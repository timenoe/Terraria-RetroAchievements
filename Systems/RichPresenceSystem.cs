using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Social.Base;
using RetroAchievements.Tools;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Used to get Rich Presence
    /// </summary>
    public class RichPresenceSystem : ModSystem
    {
        /// <summary>
        /// Get the Rich Presence of the current game state
        /// </summary>
        public static string GetRichPresence()
        {
            string player = "";
            string world = "";
            string zone = "";

            if (RichPresenceState.GetCurrentState().GameMode >= RichPresenceState.GameModeState.PlayingSingle)
            {
                player = $"Player: {PlayerTool.GetHp()} HP | {PlayerTool.GetMp()} MP | {PlayerTool.GetDifficultyStr()} | {PlayerTool.GetPlayTimeHoursStr()} | {PlayerTool.GetHeldItemStr()}";
                world = $"World: {WorldTool.GetSeedStr()} | {WorldTool.GetSizeStr()} | {WorldTool.GetDifficultyStr()} | {WorldTool.GetEvilStr()} | {WorldTool.GetTimeOfDayStr()}";
                zone = $"Biomes: {string.Join(", ", PlayerTool.GetCurrentBiomesStr())}";
            }

            Version version = ModContent.GetInstance<RetroAchievements>().Version;
            switch (RichPresenceState.GetCurrentState().GameMode)
            {
                // Display a random window title
                case RichPresenceState.GameModeState.InMainMenu:
                    return Lang.GetRandomGameTitle();

                case RichPresenceState.GameModeState.CreatingPlayer:
                    return "Creating a Player";

                case RichPresenceState.GameModeState.CreatingWorld:
                    return "Creating a World";

                case RichPresenceState.GameModeState.PlayingSingle:
                    string single = $"Playing Singleplayer{RetroAchievements.GetChallengeModeStr()} | Progression: {WorldTool.GetProgressionStr()}";
                    return $"v{version} • {single} • {player} • {world} • {zone}";

                case RichPresenceState.GameModeState.PlayingMulti:
                    string multi = $"Playing Multiplayer{RetroAchievements.GetChallengeModeStr()} | Players: {PlayerTool.GetPlayerCount()} | Progression: {WorldTool.GetProgressionStr()}";
                    return $"v{version} • {multi} • {player} • {world} • {zone}";
            }

            return "Playing Terraria";
        }
    }
}
