using Terraria;
using Terraria.ID;
using Terraria.IO;

namespace RetroAchievements.Utils
{
    /// <summary>
    /// Utility to retrieve world information
    /// </summary>
    public class WorldUtil
    {
        /// <summary>
        /// Returns true if the world is a special seed
        /// </summary>
        /// <returns>True if the world is a special seed</returns>
        public static bool IsSpecialSeed() => Main.specialSeedWorld;

        /// <summary>
        /// Returns true if the world file data is a special seed
        /// </summary>
        /// <param name="data">World data</param>
        /// <returns>Trus if the world file data is a special seed</returns>
        public static bool IsSpecialSeed(WorldFileData data)
        {
            return data.Anniversary ||
                   data.DontStarve ||
                   data.DrunkWorld ||
                   data.ForTheWorthy ||
                   data.NoTrapsWorld ||
                   data.NotTheBees ||
                   data.RemixWorld ||
                   data.ZenithWorld;
        }

        /// <summary>
        /// Returns true if the world file data is Journey Mode
        /// </summary>
        /// <param name="world">World file data</param>
        /// <returns>True if the world file data is Journey Mode</returns>
        public static bool IsJourneyMode(WorldFileData world) => world.GameMode == GameModeID.Creative;

        /// <summary>
        /// Get the name of the world<br/>
        /// World's can be manually named by the player, so don't display it in Rich Presence
        /// </summary>
        /// <returns>Name of the world</returns>
        public static string GetName() => Main.worldName;

        /// <summary>
        /// Get the seed of the world
        /// </summary>
        /// <returns>Special seed name if available; otherwise RNG seed name</returns>
        public static string GetSeed()
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

            return WorldGen.currentWorldSeed;
        }

        /// <summary>
        /// Get the size of the world
        /// </summary>
        /// <returns>Small, Medium, or Large</returns>
        public static string GetSize()
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
        public static string GetDifficulty()
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
        public static string GetEvil() => WorldGen.crimson ? "Crimson" : "Corruption";

        /// <summary>
        /// Get the time of day of the world
        /// </summary>
        /// <returns>Day or Night</returns>
        public static string GetTimeOfDay() => Main.dayTime ? "Day" : "Night";

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
