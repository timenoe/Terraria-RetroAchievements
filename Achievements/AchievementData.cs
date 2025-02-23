using System.Collections.Generic;
using RASharpIntegration.Data;

namespace RetroAchievements.Achievements
{
    // Output out Edit > Paste Special > Paste JSON as classes (renamed for accuracy)

    /// <summary>
    /// Used to identify achievement data for Terraria or a Terraria mod
    /// </summary>
    public class TerrariaAchievementData
    {
        /// <summary>
        /// Game-related information
        /// </summary>
        public TerrariaAchievementGame Game { get; set; }

        /// <summary>
        /// List of all achievements 
        /// </summary>
        public TerrariaAchievement[] Achievements { get; set; }
    }

    /// <summary>
    /// Used to identify Terraria or a Terraria mod with achievements
    /// </summary>
    public class TerrariaAchievementGame
    {
        /// <summary>
        /// List of all mods that are achievement subsets
        /// </summary>
        public Dictionary<string, string> SubsetMods { get; set; }

        /// <summary>
        /// List of all mods that are allowed with this game
        /// </summary>
        public string[] WhitelistedMods { get; set; }

        /// <summary>
        /// RA-related game information
        /// </summary>
        public RaGame Ra { get; set; }
    }

    /// <summary>
    /// Used to identify a Terraria achievement
    /// </summary>
    public class TerrariaAchievement
    {
        /// <summary>
        /// Internal name of the achievement (BENCHED, etc.)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category of the achievement (Slayer, Collector, Explorer, or Challenger)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// RA-related achievement information
        /// </summary>
        public RaAchievement Ra { get; set; }
    }
}
