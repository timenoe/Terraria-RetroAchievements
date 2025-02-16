using Terraria;
using Terraria.ID;
using Terraria.IO;

namespace RetroAchievements.Tools
{
    /// <summary>
    /// Tool to retrieve world information
    /// </summary>
    public class WorldTool
    {
        /// <summary>
        /// Get the seed of the world
        /// </summary>
        /// <returns>Special seed name if available; otherwise RNG seed name</returns>
        public static string GetSeedStr()
        {
            if (Main.specialSeedWorld)
            {
                // Zenith combines multiple special seeds, so check it first
                if (Main.zenithWorld)
                    return "Zenith";

                if (Main.drunkWorld)
                    return "Drunk World";

                if (Main.notTheBeesWorld)
                    return "Not The Bees";

                if (Main.getGoodWorld)
                    return "For The Worthy";

                if (Main.tenthAnniversaryWorld)
                    return "Celebrationmk10";

                if (Main.dontStarveWorld)
                    return "The Constant";

                if (Main.noTrapsWorld)
                    return "No Traps";

                if (Main.remixWorld)
                    return "Don't Dig Up";
            }

            // Otherwise return the normal numbered seed
            return Main.ActiveWorldFileData.Seed.ToString();
        }

        /// <summary>
        /// Get the size of the world
        /// </summary>
        /// <returns>Small, Medium, or Large</returns>
        public static string GetSizeStr()
        {
            if (WorldGen.GetWorldSize() == WorldGen.WorldSize.Small)
                return "Small";

            if (WorldGen.GetWorldSize() == WorldGen.WorldSize.Large)
                return "Large";

            return "Medium";
        }

        /// <summary>
        /// Get the difficulty of the world
        /// </summary>
        /// <returns>Journey, Normal, Expert, or Master</returns>
        public static string GetDifficultyStr()
        {
            return Main.GameModeInfo.Id switch
            {
                GameModeID.Master => "Master",
                GameModeID.Expert => "Expert",
                GameModeID.Creative => "Journey",
                _ => "Normal",
            };
        }

        /// <summary>
        /// Get the evil of the world
        /// </summary>
        /// <returns>Crimson or Corruption</returns>
        public static string GetEvilStr() => WorldGen.crimson ? "Crimson" : "Corruption";

        /// <summary>
        /// Gets the current progression state
        /// </summary>
        /// <returns>Current progression state</returns>
        public static string GetProgressionStr()
        {
            if (NPC.downedMoonlord)
                return "Post-Moon Lord";

            if (NPC.downedAncientCultist)
                return "Post-Lunatic Cultist";

            if (NPC.downedGolemBoss)
                return "Post-Golem";

            if (NPC.downedPlantBoss)
                return "Post-Plantera";

            if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                return "Post-Mechanical Bosses";

            if (Main.hardMode)
                return "Hardmode";

            return "Pre-Hardmode";
        }

        /// <summary>
        /// Get the time of day of the world
        /// </summary>
        /// <returns>Day or Night</returns>
        public static string GetTimeOfDayStr() => Main.dayTime ? "Day" : "Night";

        /// <summary>
        /// Returns true if the world is the Celebrationmk10 seed
        /// </summary>
        /// <param name="data">World data</param>
        /// <returns>True if the world is the Celebrationmk10 seed</returns>
        public static bool IsAnniversarySeed(WorldFileData data) => data.Anniversary && !data.ZenithWorld;

        /// <summary>
        /// Returns true if the world file data is Journey Mode
        /// </summary>
        /// <param name="world">World file data</param>
        /// <returns>True if the world file data is Journey Mode</returns>
        public static bool IsJourneyMode(WorldFileData world) => world.GameMode == GameModeID.Creative;

        /// <summary>
        /// Returns true if the world is single player
        /// </summary>
        /// <returns>True if the world is single player</returns>
        public static bool IsMultiplayer() => Main.netMode != NetmodeID.SinglePlayer;

        /// <summary>
        /// Check if a world was generated with a specific mod enabled
        /// </summary>
        /// <param name="world">World file data</param>
        /// <param name="modName">Mod name</param>
        /// <returns>True if the world was generated with the specific mod enabled</returns>
        public static bool WasGeneratedWithMod(WorldFileData world, string modName)
        {
            if (!world.WorldGenModsRecorded)
                return false;

            if (!world.TryGetModVersionGeneratedWith(modName, out _))
                return false;

            return true;
        }
    }
}
