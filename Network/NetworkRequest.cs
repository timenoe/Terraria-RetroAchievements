using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using RetroAchievementsMod.Achievements;
using RetroAchievementsMod.Utils;

namespace RetroAchievementsMod.Network
{
    public class RequestManager
    {
        // May be changed during runtime
        public bool hardcore = true;
        public bool mastered;
        public List<int> unlockedAchs = [];

        // Assigned during creation; most won't change
        // Except the token after potential login
        private readonly int game;
        private readonly string host;
        private readonly int totalAchs;
        private readonly string user;
        private readonly HttpClient client = new();
        private string token;

        public RequestManager(string userName, int gameId, string agentVer, string raHost = "stage.retroachievements.org", string userToken = "")
        {
            host = raHost;
            user = userName;
            token = userToken;
            game = gameId;
            totalAchs = AchievementIds.GetAchievementCount(game);

            // RA requires a valid user agent of {Standalone name}/{x.y.z Version}
            client.DefaultRequestHeaders.Add("User-Agent", $"TerrariaRetroAchievementsMod/{agentVer}");
        }

        public static void BuildLoginRequest(string host, string user, string pass, out UriBuilder builder)
        {
            builder = new($"https://{host}/dorequest.php");
            NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["r"] = "login2";
            parameters["u"] = user;
            parameters["p"] = pass;
            builder.Query = parameters.ToString();
        }

        public static void BuildStartSessionRequest(string host, string user, string token, int game, out UriBuilder builder)
        {
            builder = new($"https://{host}/dorequest.php");
            NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["r"] = "startsession";
            parameters["u"] = user;
            parameters["t"] = token;
            parameters["g"] = game.ToString();
            builder.Query = parameters.ToString();
        }

        public static void BuildPingRequest(string host, string user, string token, int game, string rp, out UriBuilder builder, out MultipartFormDataContent multipart)
        {
            builder = new($"https://{host}/dorequest.php");
            multipart = [];
            NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["r"] = "ping";
            parameters["u"] = user;
            parameters["t"] = token;
            parameters["g"] = game.ToString();
            multipart.Add(new StringContent(rp), "m");
            builder.Query = parameters.ToString();
        }

        public static void BuildAwardAchievementRequest(string host, string user, string token, bool hardcore, int ach, out UriBuilder builder)
        {
            builder = new($"https://{host}/dorequest.php");
            NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["r"] = "awardachievement";
            parameters["u"] = user;
            parameters["t"] = token;
            parameters["h"] = hardcore ? "1" : "0";
            parameters["a"] = ach.ToString();
            parameters["v"] = HashUtils.GenerateMD5($"{ach}{user}{hardcore}{ach}");
            builder.Query = parameters.ToString();
        }

        public static void BuildAwardAchievementsRequest(string host, string user, string token, bool hardcore, List<int> achs, out UriBuilder builder, out MultipartFormDataContent multipart)
        {
            builder = new($"https://{host}/dorequest.php");
            multipart = [];
            NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["r"] = "awardachievements";
            parameters["u"] = user;
            parameters["t"] = token;
            multipart.Add(new StringContent(hardcore ? "1" : "0"), "h");
            multipart.Add(new StringContent(string.Join(",", achs)), "a");
            multipart.Add(new StringContent(HashUtils.GenerateMD5($"{achs}{user}{hardcore}")), "v");
            builder.Query = parameters.ToString();
        }

        public async Task<bool> Login(string pass)
        {
            Console.WriteLine($"Logging in {user} to {host}...");

            LoginResponse response = await SendLoginRequest(pass);
            switch (response.type)
            {
                case ResponseType.SUCCESS:
                    token = response.token;
                    Console.WriteLine($"Successfully logged in {user} to {host} and cached their token");
                    return true;

                case ResponseType.ERROR:
                    Console.WriteLine($"Unable to login {user} due to error {response.error}");
                    return false;

                case ResponseType.EXCEPTION:
                    Console.WriteLine($"Unable to login {user} due to exception {response.exception}");
                    return false;

                case ResponseType.EMPTY:
                    Console.WriteLine($"Unable to login {user} due to an invalid response from RA");
                    return false;
            }
            return false;
        }

        public async Task<bool> StartSession()
        {
            Console.WriteLine($"Starting the RA session...");

            StartSessionResponse response = await SendStartSessionRequest();
            switch (response.type)
            {
                case ResponseType.SUCCESS:
                    unlockedAchs = response.unlocks;
                    Console.WriteLine($"Successfully started the RA session with {unlockedAchs.Count}/{totalAchs} achievements earned");
                    return true;

                case ResponseType.ERROR:
                    Console.WriteLine($"Unable to start session due to error {response.error}");
                    return false;

                case ResponseType.EXCEPTION:
                    Console.WriteLine($"Unable to start session due to exception {response.exception}");
                    return false;

                case ResponseType.EMPTY:
                    Console.WriteLine($"Unable to start session due to an invalid response from RA");
                    return false;
            }
            return false;
        }

        public async Task<bool> Ping(string rp)
        {
            Console.WriteLine($"Sending an activity ping to the RA server...");

            PingResponse response = await SendPingRequest(rp);
            switch (response.type)
            {
                case ResponseType.SUCCESS:
                    Console.WriteLine($"Successfully sent a ping to the RA server");
                    return true;

                case ResponseType.ERROR:
                    Console.WriteLine($"Unable to ping due to error {response.error}");
                    return false;

                case ResponseType.EXCEPTION:
                    Console.WriteLine($"Unable to ping due to exception {response.exception}");
                    return false;

                case ResponseType.EMPTY:
                    Console.WriteLine($"Unable to ping due to an invalid response from RA");
                    return false;
            }
            return false;
        }

        public async Task<bool> Unlock(int ach)
        {
            Console.WriteLine($"Unlocked achievement {ach}...");

            AwardAchievementResponse response = await SendAwardAchievementRequest(ach);
            switch (response.type)
            {
                case ResponseType.SUCCESS:
                    // TODO: Check if there are 0 achievements left to unlock
                    Console.WriteLine($"Successfully unlocked achievement {ach}");
                    return true;

                case ResponseType.ERROR:
                    Console.WriteLine($"Unable to unlock achievement {ach} due to error {response.error}");
                    return false;

                case ResponseType.EXCEPTION:
                    Console.WriteLine($"Unable to unlock achievmenent {ach} due to exception {response.exception}");
                    return false;

                case ResponseType.EMPTY:
                    Console.WriteLine($"Unable to unlock achievement {ach} due to an invalid response from RA");
                    return false;
            }
            return false;
        }

        private async Task<LoginResponse> SendLoginRequest(string pass)
        {
            BuildLoginRequest(host, user, pass, out UriBuilder request);
            Dictionary<string, dynamic> response = await SendRequest(request);
            return new LoginResponse(response);
        }

        private async Task<StartSessionResponse> SendStartSessionRequest()
        {
            BuildStartSessionRequest(host, user, token, game, out UriBuilder request);
            Dictionary<string, dynamic> response = await SendRequest(request);
            return new StartSessionResponse(response);
        }

        private async Task<PingResponse> SendPingRequest(string rp)
        {
            BuildPingRequest(host, user, token, game, rp, out UriBuilder request, out MultipartFormDataContent content);
            Dictionary<string, dynamic> response = await SendRequest(request, content);
            return new PingResponse(response);
        }

        private async Task<AwardAchievementResponse> SendAwardAchievementRequest(int ach)
        {
            BuildAwardAchievementRequest(host, user, token, hardcore, ach, out UriBuilder request);
            Dictionary<string, dynamic> response = await SendRequest(request);
            return new AwardAchievementResponse(response);
        }

        private async Task<AwardAchievementsResponse> SendAwardAchievementsRequest(List<int> achs)
        {
            BuildAwardAchievementsRequest(host, user, token, hardcore, achs, out UriBuilder request, out MultipartFormDataContent multipart);
            Dictionary<string, dynamic> response = await SendRequest(request, multipart);
            return new AwardAchievementsResponse(response);
        }

        private async Task<Dictionary<string, dynamic>> SendRequest(UriBuilder request, MultipartFormDataContent? multipart = null)
        {
            try
            {
                // Console.WriteLine($"Raw API request: {request.Uri}");
                multipart ??= []; // If no multipart data was provided, create empty collection
                HttpResponseMessage response = await client.PostAsync(request.Uri, multipart);
                string content = await response.Content.ReadAsStringAsync();

                //Console.WriteLine($"Raw API response: {content}");
                // Don't add null values to resulting Dictionary
                // Return empty Dictionary if response if invalid
                return JsonUtil.DeserializeJson(content);
            }

            catch (HttpRequestException e)
            {
                // Exception can occur if the client's internet is down, there are DNS issues, etc...
                return new Dictionary<string, dynamic> { { "Exception", $"{e.GetType()}: {e.Message}" } };
            }
        }
    }
}
