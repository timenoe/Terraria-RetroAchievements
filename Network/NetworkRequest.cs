using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using RetroAchievementsMod.Utils;

namespace RetroAchievementsMod.Network
{
    public class NetworkRequest
    {
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
        public static async Task<Dictionary<string, dynamic>> SendApiRequest(HttpClient client, UriBuilder request, MultipartFormDataContent multipart = null)
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
