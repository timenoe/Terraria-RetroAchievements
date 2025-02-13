using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.IO;

namespace RetroAchievements.Tools
{
    /// <summary>
    /// Tool to retrieve player information
    /// </summary>
    public class PlayerTool
    {
        /// <summary>
        /// Get the HP of the player
        /// </summary>
        /// <returns>HP of the player</returns>
        public static int GetHp()
        {
            Player player = Main.LocalPlayer;
            if (player == null)
                return 0;

            return player.statLife;
        }

        /// <summary>
        /// Get the MP of the player
        /// </summary>
        /// <returns>MP of the player</returns>
        public static int GetMp()
        {
            Player player = Main.LocalPlayer;
            if (player == null)
                return 0;

            return player.statMana;
        }

        /// <summary>
        /// Returns the total player count in multiplayer
        /// </summary>
        /// <returns>Total player count in multiplayer</returns>
        public static int GetPlayerCount() => Main.player.Take(Main.maxPlayers).Count(p => p.active);

        /// <summary>
        /// Get the difficulty of the player
        /// </summary>
        /// <returns>Journey, Classic, Mediumcore, or Hardcore</returns>
        public static string GetDifficultyStr()
        {
            Player player = Main.LocalPlayer;
            if (player == null)
                return "";

            return player.difficulty switch
            {
                PlayerDifficultyID.Creative => "Journey",
                PlayerDifficultyID.MediumCore => "Mediumcore",
                PlayerDifficultyID.Hardcore => "Hardcore",
                _ => "Classic"
            };
        }

        /// <summary>
        /// Get the play time of the player
        /// </summary>
        /// <returns>Play time of the player (formatted as hh:mm:ss)</returns>
        public static string GetPlayTimeStr()
        {
            PlayerFileData data = Main.ActivePlayerFileData;
            if (data == null)
                return "";

            // Return without fractional seconds
            return data.GetPlayTime().ToString().Split(".")[0];
        }

        /// <summary>
        /// Get the name of the item that the player if holding
        /// </summary>
        /// <returns>Name of the item that the player is holding; No Item if none</returns>
        public static string GetHeldItemStr()
        {
            Player player = Main.LocalPlayer;
            if (player == null)
                return "";

            Item item = Main.LocalPlayer.HeldItem;
            if (item == null || string.IsNullOrEmpty(item.Name))
                return "No Held Item";

            return item.Name;
        }

        /// <summary>
        /// Get a list of all the zones that the player is in
        /// </summary>
        /// <returns>List of all zones that the player is in</returns>
        public static List<string> GetCurrentBiomesStr()
        {
            Player player = Main.LocalPlayer;
            List<string> zones = [];

            if (player == null)
                return zones;

            if (player.ZoneBeach)
                zones.Add("Beach");

            if (player.ZoneCorrupt)
                zones.Add("Corruption");

            if (player.ZoneCrimson)
                zones.Add("Crimson");

            if (player.ZoneDesert)
                zones.Add("Desert");

            if (player.ZoneDirtLayerHeight)
                zones.Add("Dirt Layer");

            if (player.ZoneDungeon)
                zones.Add("Dungeon");

            if (player.ZoneForest)
                zones.Add("Forest");

            if (player.ZoneGemCave)
                zones.Add("Gem Cave");

            if (player.ZoneGlowshroom)
                zones.Add("Glowshroom");

            if (player.ZoneGranite)
                zones.Add("Granite");

            if (player.ZoneGraveyard)
                zones.Add("Graveyard");

            if (player.ZoneHallow)
                zones.Add("Hallow");

            if (player.ZoneHive)
                zones.Add("Hive");

            if (player.ZoneJungle)
                zones.Add("Jungle");

            if (player.ZoneLihzhardTemple)
                zones.Add("Lihzhard Temple");

            if (player.ZoneMarble)
                zones.Add("Marble");

            if (player.ZoneMeteor)
                zones.Add("Meteor");

            if (player.ZoneNormalCaverns)
                zones.Add("Caverns");

            if (player.ZoneNormalSpace)
                zones.Add("Space");

            if (player.ZoneNormalUnderground)
                zones.Add("Underground");

            if (player.ZoneOldOneArmy)
                zones.Add("Old One Army");

            if (player.ZoneOverworldHeight)
                zones.Add("Overworld");

            if (player.ZonePeaceCandle)
                zones.Add("Peace Candle");

            if (player.ZonePurity)
                zones.Add("Purity");

            if (player.ZoneRain)
                zones.Add("Rain");

            if (player.ZoneRockLayerHeight)
                zones.Add("Rock Layer");

            if (player.ZoneSandstorm)
                zones.Add("Sandstorm");

            if (player.ZoneShadowCandle)
                zones.Add("Shadow Candle");

            if (player.ZoneShimmer)
                zones.Add("Shimmer");

            if (player.ZoneSkyHeight)
                zones.Add("Sky");

            if (player.ZoneSnow)
                zones.Add("Snow");

            if (player.ZoneTowerNebula)
                zones.Add("Tower Nebula");

            if (player.ZoneTowerSolar)
                zones.Add("Tower Solar");

            if (player.ZoneTowerStardust)
                zones.Add("Tower Stardust");

            if (player.ZoneTowerVortex)
                zones.Add("Tower Vortex");

            if (player.ZoneUndergroundDesert)
                zones.Add("Underground Desert");

            if (player.ZoneUnderworldHeight)
                zones.Add("Underworld");

            if (player.ZoneWaterCandle)
                zones.Add("Water Candle");

            return zones;
        }
    }
}
