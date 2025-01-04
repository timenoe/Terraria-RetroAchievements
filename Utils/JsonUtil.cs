using System.Collections.Generic;
using Newtonsoft.Json;

namespace RetroAchievementsMod.Utils
{
    public static class JsonUtil
    {
        // Convert a JSON string to a Dictionary
        public static Dictionary<string, dynamic> DeserializeJson(string str)
        {
            JsonSerializerSettings settings = new() { NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(str, settings) ?? [];
        }
    }
}
