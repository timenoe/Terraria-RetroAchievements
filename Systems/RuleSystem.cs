using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Achievements;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using RetroAchievements.Players;
using RetroAchievements.Utils;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Used to enforce rules and manage in-game achievements
    /// </summary>
    public class RuleSystem : ModSystem
    {
        /// <summary>
        /// True if the user has selected Multiplayer
        /// </summary>
        private bool _hasSelectedMulti;

        /// <summary>
        /// Name of the mod to display in the mod rejection message
        /// </summary>
        private string _rejectedMod;

        /// <summary>
        /// Reason that the selected world was rejected
        /// </summary>
        private RejectionReason _rejectionReason;


        /// <summary>
        /// Event to unlock an achievement for the user
        /// </summary>
        public event EventHandler<UnlockAchievementEventArgs> UnlockAchievementCommand;


        /// <summary>
        /// Used to identify why a selected world was rejected
        /// </summary>
        private enum RejectionReason { NONE, MODS, MULTI, PLAYER, WORLD, JOURNEY, SEED };


        public override void OnModLoad()
        {
            if (!RetroAchievements.IsEnabled)
                return;
            
            Main.Achievements.OnAchievementCompleted += Achievements_OnAchievementCompleted;
            Main.OnPostDraw += Main_OnPostDraw;
        }

        public override void OnModUnload()
        {
            if (!RetroAchievements.IsEnabled)
                return;

            Main.Achievements.OnAchievementCompleted -= Achievements_OnAchievementCompleted;
            Main.OnPostDraw -= Main_OnPostDraw;
        }

        public override bool CanWorldBePlayed(PlayerFileData playerData, WorldFileData worldFileData)
        {
            if (!RetroAchievements.IsEnabled)
                return true;

            // Check if any disallowed mods are loaded
            foreach (Mod mod in ModLoader.Mods)
            {
                if (!RetroAchievements.IsModAllowed(mod))
                {
                    _rejectedMod = mod.DisplayNameClean;
                    _rejectionReason = RejectionReason.MODS;
                    return false;
                }
            }

            // Check if Multiplayer > Host & Play was selected
            // Join via IP/Steam does not select a world
            // So they are checked for in AchievementPlayer.OnEnterWorld
            if (!RetroAchievements.IsMultiAllowed() && _hasSelectedMulti)
            {
                _rejectionReason = RejectionReason.MULTI;
                return false;
            }

            // Check that Player was generated with this mod
            RetroAchievementPlayer player = playerData.Player.GetModPlayer<RetroAchievementPlayer>();
            if (!player.WasCreatedWithRa && !PlayerUtil.HasNoPlayTime(playerData))
            {
                _rejectionReason = RejectionReason.PLAYER;
                return false;
            }

            // Check that the World was generated with this mod
            if (!WorldUtil.WasGeneratedWithMod(worldFileData, "RetroAchievements"))
            {
                _rejectionReason = RejectionReason.WORLD;
                return false;
            }

            // Check that Journey Mode is not being used
            if (WorldUtil.IsJourneyMode(worldFileData))
            {
                _rejectionReason = RejectionReason.JOURNEY;
                return false;
            }

            // Check that the World is not Celebrationmk10
            if (worldFileData.Anniversary)
            {
                _rejectionReason = RejectionReason.SEED;
                return false;
            }

            _rejectionReason = RejectionReason.NONE;
            return true;
        }

        public override string WorldCanBePlayedRejectionMessage(PlayerFileData playerData, WorldFileData worldData)
        {
            switch (_rejectionReason)
            {
                case RejectionReason.MODS:
                    return $"RetroAchievements: Cannot play with {_rejectedMod}";

                case RejectionReason.MULTI:
                    return "RetroAchievements: Cannot play Multiplayer";

                case RejectionReason.PLAYER:
                    return "RetroAchievements: Cannot play a Player from another mod";

                case RejectionReason.WORLD:
                    return "RetroAchievements: Cannot play a World from another mod";

                case RejectionReason.JOURNEY:
                    return "RetroAchievements: Cannot play Journey Mode";

                case RejectionReason.SEED:
                    return "RetroAchievements: Cannot play a World with the Celebrationmk10 seed";
                
                case RejectionReason.NONE:
                    break;

                default:
                    return "RetroAchievements: Cannot play for an undefined reason";
            }

            return "";
        }

        /// <summary>
        /// Achievements.OnAchievementCompleted event callback to process in-game achievement unlocks
        /// </summary>
        /// <param name="achievement">Achievement information</param>
        private void Achievements_OnAchievementCompleted(Achievement achievement)
        {
            RetroAchievementPlayer player = Main.LocalPlayer.GetModPlayer<RetroAchievementPlayer>();
            if (player == null || !player.CanEarnAchievements)
                return;

            UnlockAchievementCommand.Invoke(this, new UnlockAchievementEventArgs(achievement.Name));
        }

        /// <summary>
        /// Main.OnAchievementCompleted event callback to disallow the selection of Multiplayer
        /// </summary>
        /// <param name="gameTime">GameTime information</param>
        private void Main_OnPostDraw(GameTime gameTime)
        {
            // Only need to run in main menus
            if (!Main.gameMenu)
                return;

            if (Main.menuMode == MenuID.Multiplayer)
                _hasSelectedMulti = true;

            else if (Main.menuMode == MenuID.Title)
                _hasSelectedMulti = false;
        }
    }
}
