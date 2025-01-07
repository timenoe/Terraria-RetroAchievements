using Terraria;
using Terraria.ModLoader;
using Terraria.Social.Base;
using RetroAchievements.Utils;

namespace RetroAchievements.Systems
{
    public class RichPresenceSystem : ModSystem
    {
        /// <summary>
        /// Get the Rich Presence to reflect the current game state
        /// </summary>
        public static string GetRichPresence()
        {
            switch (RichPresenceState.GetCurrentState().GameMode)
            {
                case RichPresenceState.GameModeState.InMainMenu:
                    return Lang.GetRandomGameTitle();

                case RichPresenceState.GameModeState.CreatingPlayer:
                    return "Creating a Player";

                case RichPresenceState.GameModeState.CreatingWorld:
                    return "Creating a World";

                case RichPresenceState.GameModeState.PlayingSingle:
                    string player = $"Player: {PlayerUtil.GetHp()} HP | {PlayerUtil.GetMp()} MP | {PlayerUtil.GetDifficulty()} | {PlayerUtil.GetPlayTime()} | {PlayerUtil.GetHeldItemName()}";
                    string world = $"World: Seed: {WorldUtil.GetSeed()} | {WorldUtil.GetSize()} | {WorldUtil.GetDifficulty()} | {WorldUtil.GetEvil()} | {WorldUtil.GetTimeOfDay()}";
                    string zones = $"Zones: {string.Join(", ", PlayerUtil.GetCurrentZones())}";
                    return $"{player} • {world} • {zones}";

                case RichPresenceState.GameModeState.PlayingMulti:
                    return "Playing Multiplayer";
            }

            return "Playing Terraria";
        }
    }
}
