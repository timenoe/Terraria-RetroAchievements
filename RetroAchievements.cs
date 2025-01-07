using System.Linq;
using System.Text.Json;
using Terraria.ModLoader;
using RetroAchievements.Achievements;
using RetroAchievements.Configs;
using RetroAchievements.Utils;
using Terraria.ID;
using Terraria;

namespace RetroAchievements
{
    /// <summary>
    /// Game to load achievement data for<br>
    /// Player chooses which game to load in the settings config
    /// </summary>
    public enum AchievementGame { None, Vanilla };

    /// <summary>
    /// Main class for the mod
    /// </summary>
    public class RetroAchievements : Mod
    {
        /// <summary>
        /// True if RA achievements are enabled
        /// </summary>
        private static bool _isEnabled;

        /// <summary>
        /// True if RA Hardcore Mode is enabled<br/>
        /// Player can disable Hardcore Mode in the settings config
        /// </summary>
        private static bool _isHardcore;

        /// <summary>
        /// Name of the RA host<br/>
        /// Player can set the host in the settings config
        /// </summary>
        private static string _host = "";

        /// <summary>
        /// Game that RA achievements are enabled for<br/>
        /// Achievements can only be enabled for one game at a time<br/>
        /// Subsets do not count as different games
        /// </summary>
        private static AchievementGame _game;

        private static RetroAchievements _instance;

        /// <summary>
        /// Achievement data that is deserialized from a JSON file
        /// </summary>
        private static TerrariaAchievementData _achievementData;


        /// <summary>
        /// True if RA achievements are enabled
        /// </summary>
        public static bool IsEnabled
        {
            get { return _isEnabled; }
        }

        /// <summary>
        /// True if RA Hardcore Mode is enabled
        /// </summary>
        public static bool IsHardcore
        {
            get { return _isHardcore; }
        }

        /// <summary>
        /// Name of the RA host
        /// </summary>
        public static string Host
        {
            get { return _host; }
        }

        /// <summary>
        /// Game that RA achievements are enabled for
        /// </summary>
        public static AchievementGame Game
        {
            get { return _game; }
        }

        public static RetroAchievements Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Achievement data that is deserialized from a JSON file
        /// </summary>
        public static TerrariaAchievementData AchievementData
        {
            get { return _achievementData; }
        }


        /// <summary>
        /// Get the RA achievement ID from the internal achievement name
        /// </summary>
        /// <param name="name">Internal name of the achievement (BENCHED, etc.)</param>
        /// <returns>RA achievement ID from the achievement name; 0 if not found</returns>
        public static int GetAchievementId(string name)
        {
            if (!IsEnabled)
                return 0;

            foreach (var achievement in AchievementData.Achievements)
            {
                if (achievement.Name == name)
                    return achievement.Ra.Id;
            }

            return 0;
        }

        /// <summary>
        /// Get the RA game ID for the current game
        /// </summary>
        /// <returns>RA game ID of the current game</returns>
        public static int GetGameId()
        {
            if (!IsEnabled)
                return 0;

            return AchievementData.Game.Ra.Id;
        }

        /// <summary>
        /// Get the RA game name for the current game
        /// </summary>
        /// <returns>RA game name of the current game</returns>
        public static string GetGameName()
        {
            if (!IsEnabled)
                return "";

            return AchievementData.Game.Ra.Name;
        }

        /// <summary>
        /// Returns true if a specific mod is allowed for the current game
        /// </summary>
        /// <param name="mod">Mod in question</param>
        /// <returns>True if the mod if allowed</returns>
        public static bool IsModAllowed(Mod mod)
        {
            if (!IsEnabled || !IsHardcore || mod.Name == "ModLoader" || mod.Name == "RetroAchievements")
                return true;

            return AchievementData.Game.AllowedMods.Contains(mod.Name);
        }

        /// <summary>
        /// Returns true if Multiplayer is allowed for the current game
        /// </summary>
        /// <returns>True if Multiplayer is allowed</returns>
        public static bool IsMultiAllowed()
        {
            if (!IsEnabled || !IsHardcore)
                return true;

            return AchievementData.Game.IsMultiAllowed;
        }

        /// <summary>
        /// Returns true if the user is in Single Player mode
        /// </summary>
        /// <returns>True if the user is in Single Player mode</returns>
        public static bool IsSinglePlayer()
        {
            return Main.netMode == NetmodeID.SinglePlayer;
        }

        public static int GetAchievementCount()
        {
            if (!IsEnabled)
                return 0;

            return AchievementData.Achievements.Length;
        }

        /// <summary>
        /// Get the expected path to the achievement data for the current game<br>
        /// If new games are added, you must ensure there is achievement data for it
        /// </summary>
        /// <param name="game">Game to get data path for</param>
        /// <returns>Path to the achievement data for the current game</returns>
        private static string GetAchievementDataPath(AchievementGame game)
        {
            return $"RetroAchievements/Achievements/Data/{game}AchievementData.json";
        }

        /// <summary>
        /// Load all user input from the config files
        /// </summary>
        private static void LoadConfigs()
        {
            MessageUtil.ModLog($"Loading data from the settings config file...");
            SettingsConfig config = ModContent.GetInstance<SettingsConfig>();

            _game = config.Game;
            switch (Game)
            {
                case AchievementGame.Vanilla:
                    byte[] dataBytes = ModContent.GetFileBytes(GetAchievementDataPath(Game));
                    _achievementData = JsonSerializer.Deserialize<TerrariaAchievementData>(dataBytes);
                    MessageUtil.ModLog($"Game set to {Game}; loaded achievement data for {AchievementData.Game.Ra.Name}");

                    _host = config.Host;
                    MessageUtil.ModLog($"Set the host to {Host}");

                    _isHardcore = config.Hardcore;
                    MessageUtil.ModLog($"Hardcore is set to {IsHardcore}");

                    _isEnabled = true;
                    break;

                default:
                    MessageUtil.ModLog($"Game set to None; achievements are disabled");
                    break;
            }
        }
        public override void Load()
        {
            _instance = this;
            LoadConfigs();
        }
    }
}
