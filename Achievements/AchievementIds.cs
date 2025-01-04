using System.Collections.Generic;

namespace RetroAchievementsMod.Achievements
{
    public static class AchievementIds
    {
        const int TerrariaGameId = 32123;

        public static readonly Dictionary<string, int> terrariaIds = new()
        {
            { "TIMBER",        483244 },
            { "BENCHED",       483245 },
            { "OBTAIN_HAMMER", 483246 }
        };

        public static int GetAchievementCount(int gameId)
        {
            switch (gameId)
            {
                case TerrariaGameId:
                    return terrariaIds.Count;

                default:
                    break;
            }

            return 0;
        }

        public static int GetAchievementId(int gameId, string achName)
        {
            switch (gameId)
            {
                case TerrariaGameId:
                    if (terrariaIds.TryGetValue(achName, out int id))
                        return id;
                    break;

                default:
                    break;
            }

            return 0;
        }
    }
}
