using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RetroAchievementsMod.Achievements;

namespace RetroAchievementsMod.Network
{
    public class NetworkClient
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

        public NetworkClient(string userName, int gameId, string agentVer, string raHost = "stage.retroachievements.org", string userToken = "")
        {
            host = raHost;
            user = userName;
            token = userToken;
            game = gameId;
            totalAchs = AchievementIds.GetAchievementCount(game);

            // RA requires a valid user agent of {Standalone name}/{x.y.z Version}
            client.DefaultRequestHeaders.Add("User-Agent", $"TerrariaRetroAchievementsMod/{agentVer}");
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
            NetworkRequest.BuildLoginRequest(host, user, pass, out UriBuilder request);
            Dictionary<string, dynamic> response = await NetworkRequest.SendApiRequest(client, request);
            return new LoginResponse(response);
        }

        private async Task<StartSessionResponse> SendStartSessionRequest()
        {
            NetworkRequest.BuildStartSessionRequest(host, user, token, game, out UriBuilder request);
            Dictionary<string, dynamic> response = await NetworkRequest.SendApiRequest(client, request);
            return new StartSessionResponse(response);
        }

        private async Task<PingResponse> SendPingRequest(string rp)
        {
            NetworkRequest.BuildPingRequest(host, user, token, game, rp, out UriBuilder request, out MultipartFormDataContent content);
            Dictionary<string, dynamic> response = await NetworkRequest.SendApiRequest(client, request, content);
            return new PingResponse(response);
        }

        private async Task<AwardAchievementResponse> SendAwardAchievementRequest(int ach)
        {
            NetworkRequest.BuildAwardAchievementRequest(host, user, token, hardcore, ach, out UriBuilder request);
            Dictionary<string, dynamic> response = await NetworkRequest.SendApiRequest(client, request);
            return new AwardAchievementResponse(response);
        }

        private async Task<AwardAchievementsResponse> SendAwardAchievementsRequest(List<int> achs)
        {
            NetworkRequest.BuildAwardAchievementsRequest(host, user, token, hardcore, achs, out UriBuilder request, out MultipartFormDataContent multipart);
            Dictionary<string, dynamic> response = await NetworkRequest.SendApiRequest(client, request, multipart);
            return new AwardAchievementsResponse(response);
        }
    }
}
