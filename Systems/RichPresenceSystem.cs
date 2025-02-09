using Terraria;
using Terraria.ModLoader;
using Terraria.Social.Base;
using RetroAchievements.Utils;

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
                player = $"Player: {PlayerUtil.GetHp()} HP | {PlayerUtil.GetMp()} MP | {PlayerUtil.GetDifficultyStr()} | {PlayerUtil.GetPlayTimeStr()} | {PlayerUtil.GetHeldItemStr()}";
                world = $"World: Seed: {WorldUtil.GetSeedStr()} | {WorldUtil.GetSizeStr()} | {WorldUtil.GetDifficultyStr()} | {WorldUtil.GetEvilStr()} | {WorldUtil.GetTimeOfDayStr()}";
                zone = $"Biomes: {string.Join(", ", PlayerUtil.GetCurrentBiomesStr())}";
            }
            
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
                    string single = $"Playing Singleplayer ({RetroAchievements.GetChallengeModeStr()}) | Progression: {WorldUtil.GetProgressionStr()}";
                    return $"{single} • {player} • {world} • {zone}";

                case RichPresenceState.GameModeState.PlayingMulti:
                    string multi = $"Playing Multiplayer ({RetroAchievements.GetChallengeModeStr()}) | Players: {PlayerUtil.GetPlayerCount()} | Progression: {WorldUtil.GetProgressionStr()}";
                    return $"{multi} • {player} • {world} • {zone}";
            }

            return "Playing Terraria";
        }
    }
}
