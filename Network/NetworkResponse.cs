using System.Collections.Generic;

namespace RetroAchievementsMod.Network
{
    public enum ResponseType { SUCCESS, ERROR, EXCEPTION, EMPTY }

    public class PingResponse
    {
        public bool success;
        public long status;
        public string code = "";
        public string error = "";
        public string exception = "";
        public ResponseType type;

        /* ping success response example
            "Success": true
        */
        /* ping error response example
            "Success": false,
            "Status": 401,
            "Code": "invalid_credentials",
            "Error": "Invalid user/password combination. Please try again."
        */
        public PingResponse(Dictionary<string, dynamic> response)
        {
            if (response.TryGetValue("Success", out var val) && val is bool)
                success = val;

            if (response.TryGetValue("Status", out val) && val is long)
                status = val;

            if (response.TryGetValue("Code", out val) && val is string)
                code = val;

            if (response.TryGetValue("Error", out val) && val is string)
                error = val;

            if (response.TryGetValue("Exception", out val) && val is string)
                exception = val;

            if (!success)
            {
                if (!string.IsNullOrEmpty(error))
                    type = ResponseType.ERROR;

                else if (!string.IsNullOrEmpty(exception))
                    type = ResponseType.EXCEPTION;

                else
                    type = ResponseType.EMPTY;
            }
        }
    }

    public class LoginResponse : PingResponse
    {
        public long messages;
        public long perms;
        public long score;
        public long softScore;
        public string accountType = "";
        public string token = "";
        public string user = "";

        /* login2 response example
            "Success": true,
            "User": "OldSchoolRunescape",
            "Token": "4AotgGxjIH5iT1gz", // Store this token in ModConfig
            "Score": 1,
            "SoftcoreScore": 0,
            "Messages": 0,
            "Permissions": 1,
            "AccountType": "Registered"
        */
        public LoginResponse(Dictionary<string, dynamic> response) : base(response)
        {
            if (response.TryGetValue("Messages", out var val) && val != null && val is long)
                messages = val;

            if (response.TryGetValue("Permissions", out val) && val is long)
                perms = val;

            if (response.TryGetValue("Score", out val) && val is long)
                score = val;

            if (response.TryGetValue("SoftcoreScore", out val) && val is long)
                softScore = val;

            if (response.TryGetValue("AccountType", out val) && val is string)
                accountType = val;

            if (response.TryGetValue("Token", out val) && val is string)
                token = val;

            if (response.TryGetValue("User", out val) && val is string)
                user = val;
        }
    }

    public class StartSessionResponse : PingResponse
    {
        public long serverTime;
        public List<int> unlocks = []; // Unlocked achievement IDs

        /* startsession response example
            "Success": true,
            "HardcoreUnlocks": [
            {
                "ID": 141,
                "When": 1591132445
            }
            ],
            "ServerNow": 1704076711
        */
        public StartSessionResponse(Dictionary<string, dynamic> response) : base(response)
        {
            if (response.TryGetValue("ServerNow", out var val) && val is long)
                serverTime = val;

            // Don't need unlock timestamps; just grab the IDs
            if (response.TryGetValue("HardcoreUnlocks", out val) && val is Newtonsoft.Json.Linq.JArray)
            {
                var unlocks = val.ToObject<List<Dictionary<string, int>>>();
                foreach (Dictionary<string, int> unlock in unlocks)
                    foreach (KeyValuePair<string, int> unlockPair in unlock)
                        if (unlockPair.Key == "ID")
                            this.unlocks.Add(unlockPair.Value);
            }
        }
    }

    public class AwardAchievementResponse : PingResponse
    {
        public long achsRemaining;
        public long achId;
        public long score;
        public long softScore;

        /* awardachievement response example
            "Success": true,
            "AchievementsRemaining": 5,
            "Score": 22866,
            "SoftcoreScore": 5,
            "AchievementID": 9 // New unlock
        */
        public AwardAchievementResponse(Dictionary<string, dynamic> response) : base(response)
        {
            if (response.TryGetValue("AchievementsRemaining", out var val) && val is long)
                achsRemaining = val;

            if (response.TryGetValue("AchievementID", out val) && val is long)
                achId = val;

            if (response.TryGetValue("Score", out val) && val is long)
                score = val;

            if (response.TryGetValue("SoftcoreScore", out val) && val is long)
                softScore = val;
        }
    }

    public class AwardAchievementsResponse : PingResponse
    {
        public long score;
        public long softScore;
        public List<int> existingIds = [];
        public List<int> successfulIds = [];

        /* awardachievements response example
            "Success": true,
            "Score": 22890,
            "SoftcoreScore": 5,
            "ExistingIDs": [141, 147],       // Were already unlocked
            "SuccessfulIDs": [142, 145, 146] // New unlocks
        */
        public AwardAchievementsResponse(Dictionary<string, dynamic> response) : base(response)
        {
            if (response.TryGetValue("Score", out var val) && val is long)
                score = val;

            if (response.TryGetValue("SoftcoreScore", out val) && val is long)
                softScore = val;

            if (response.TryGetValue("ExistingIDs", out val) && val is Newtonsoft.Json.Linq.JArray)
                existingIds = val.ToObject<List<int>>();

            if (response.TryGetValue("SuccessfulIDs", out val) && val is Newtonsoft.Json.Linq.JArray)
                successfulIds = val.ToObject<List<int>>();
        }
    }
}
