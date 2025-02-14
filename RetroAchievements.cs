using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Terraria;
using Terraria.ModLoader;
using RetroAchievements.Achievements;
using RetroAchievements.Configs;
using RetroAchievements.Tools;

namespace RetroAchievements
{
    /// <summary>
    /// Game to load achievement data for<br>
    /// Player chooses which game to load in the settings config
    /// </summary>
    public enum AchievementGame { Terraria };

    /// <summary>
    /// Main class for the mod
    /// </summary>
    public class RetroAchievements : Mod
    {
        private static readonly List<string> _internalMods = ["ModLoader", "RetroAchievements"];
        
        /// <summary>
        /// True if RA Hardcore Mode is enabled<br/>
        /// Hardcore is disabled if multiplayer is enabled in the settings config
        /// </summary>
        private static bool _isHardcore;

        /// <summary>
        /// Name of the RA host<br/>
        /// Player can set the host in the settings config
        /// </summary>
        private static string _host = "retroachievements.org";

        /// <summary>
        /// Game that RA achievements are enabled for<br/>
        /// Achievements can only be enabled for one game at a time<br/>
        /// Subsets do not count as different games
        /// </summary>
        private static AchievementGame _game;

        /// <summary>
        /// Achievement data that is deserialized from a JSON file
        /// </summary>
        private static TerrariaAchievementData _achievementData;

        /// <summary>
        /// True if RA Hardcore Mode is enabled
        /// </summary>
        public static bool IsHardcore => _isHardcore;

        /// <summary>
        /// Name of the RA host
        /// </summary>
        public static string Host => _host;

        /// <summary>
        /// Game that RA achievements are enabled for
        /// </summary>
        public static AchievementGame Game => _game;

        /// <summary>
        /// Achievement data that is deserialized from a JSON file
        /// </summary>
        public static TerrariaAchievementData AchievementData => _achievementData;

        public override void Load()
        {
            if (Main.dedServ)
                return;
#if DEBUG
            _internalMods.Add("CheatSheet");
#endif
            LoadConfigs();
        }

        /// <summary>
        /// Get the RA achievement ID from the internal achievement name
        /// </summary>
        /// <param name="name">Internal name of the achievement (BENCHED, etc.)</param>
        /// <returns>RA achievement ID from the achievement name; 0 if not found</returns>
        public static int GetAchievementId(string name)
        {
            foreach (var achievement in AchievementData.Achievements)
            {
                if (achievement.Name == name)
                    return achievement.Ra.Id;
            }

            return 0;
        }

        /// <summary>
        /// Returns the status of Challenge Mode
        /// </summary>
        /// <returns>Challenge Mode: Enabled/Disabled</returns>
        public static string GetChallengeModeStr() => IsHardcore ? " (Challenge Mode)" : "";

        /// <summary>
        /// Get the RA game ID for the current game
        /// </summary>
        /// <returns>RA game ID of the current game</returns>
        public static int GetGameId() => AchievementData.Game.Ra.Id;

        /// <summary>
        /// Get the RA game name for the current game
        /// </summary>
        /// <returns>RA game name of the current game</returns>
        public static string GetGameName() => AchievementData.Game.Ra.Name;

        /// <summary>
        /// Get the IDs of the sets associated with this game
        /// </summary>
        /// <returns>IDs of the sets associated with this game</returns>
        public static int[] GetSets() => AchievementData.Game.Ra.Sets;

        /// <summary>
        /// Returns true if an achievement is in core on the RA server
        /// </summary>
        /// <param name="name">Internal name of the achievement (BENCHED, etc.)</param>
        /// <returns>True if the achievement is in core on the RA server</returns>
        public static bool IsCoreAchievement(string name)
        {
            foreach (var achievement in AchievementData.Achievements)
            {
                if (achievement.Name == name)
                    return achievement.Ra.Category == 3;
            }

            return false;
        }

        /// <summary>
        /// Returns true if a specific mod is allowed for the current game
        /// </summary>
        /// <param name="mod">Mod in question</param>
        /// <returns>True if the mod if allowed</returns>
        public static bool IsModAllowed(Mod mod) => _internalMods.Contains(mod.Name) || AchievementData.Game.AllowedMods.Contains(mod.Name);

        /// <summary>
        /// Gets the total achievement count for the current game
        /// </summary>
        /// <returns>Achievement count for the current game</returns>
        public static int GetAchievementCount() => AchievementData.Achievements.Length;

        /// <summary>
        /// Get the expected path to the achievement data for the current game<br>
        /// If new games are added, you must ensure there is achievement data for it
        /// </summary>
        /// <param name="game">Game to get data path for</param>
        /// <returns>Path to the achievement data for the current game</returns>
        private static string GetAchievementDataPath(AchievementGame game) => $"RetroAchievements/Achievements/Data/{game}AchievementData.json";

        /// <summary>
        /// Load all user input from the config files
        /// </summary>
        private static void LoadConfigs()
        {
            MessageTool.ModLog($"Loading data from the settings config...");
            SettingsConfig config = ModContent.GetInstance<SettingsConfig>();

            _host = config.Host;
            MessageTool.ModLog($"RetroAchievements host set to {Host}");

            _isHardcore = config.ChallengeMode;
            MessageTool.ModLog($"Hardcore is set to {IsHardcore}");

            _game = config.Game;
            switch (Game)
            {
                case AchievementGame.Terraria:
                    byte[] bytes = ModContent.GetFileBytes(GetAchievementDataPath(Game));
                    _achievementData = JsonSerializer.Deserialize<TerrariaAchievementData>(bytes);
                    MessageTool.ModLog($"Loaded achievement data for {Game}");
                    break;
            }
        }
    }
}
