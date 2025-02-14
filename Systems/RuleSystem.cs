using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Achievements;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using RetroAchievements.Players;
using RetroAchievements.Configs;
using RetroAchievements.Tools;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Used to enforce rules and manage in-game achievements
    /// </summary>
    public class RuleSystem : ModSystem
    {
        /// <summary>
        /// True if the world was used in multiplayer with RA enabled
        /// </summary>
        private static bool _isRaMultiWorld;

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
        /// True if the world was used in multiplayer with RA enabled
        /// </summary>
        public static bool IsRaMultiWorld => _isRaMultiWorld;


        /// <summary>
        /// Used to identify why a selected world was rejected
        /// </summary>
        private enum RejectionReason
        {
            None,
            JourneyMode,
            AnniversarySeed,
            MultiPlayer,
            MultiWorld,
            ExternalPlayer,
            ExternalWorld,
            ExternalMod
        };


        public override void OnModLoad()
        {
            if (Main.dedServ)
                return;
            
            Main.Achievements.OnAchievementCompleted += Achievements_OnAchievementCompleted;
            Main.OnPostDraw += Main_OnPostDraw;
            On_PlayerFileData.CreateAndSave += On_PlayerFileData_CreateAndSave;
        }

        public override void OnModUnload()
        {
            if (Main.dedServ)
                return;

            Main.Achievements.OnAchievementCompleted -= Achievements_OnAchievementCompleted;
            Main.OnPostDraw -= Main_OnPostDraw;
            On_PlayerFileData.CreateAndSave -= On_PlayerFileData_CreateAndSave;
        }

        public override bool CanWorldBePlayed(PlayerFileData playerData, WorldFileData worldFileData)
        {
            _rejectionReason = RejectionReason.None;

            // Allow everything in RA Softcore Mode
            if (!RetroAchievements.IsHardcore)
                return true;
            
            // Check that Journey Mode is not being used
            if (WorldTool.IsJourneyMode(worldFileData))
            {
                _rejectionReason = RejectionReason.JourneyMode;
                return false;
            }

            // Check that the world is not using the Celebrationmk10 seed
            if (WorldTool.IsAnniversarySeed(worldFileData))
            {
                _rejectionReason = RejectionReason.AnniversarySeed;
                return false;
            }

            // Check that a multiplayer player is not being used
            RetroAchievementPlayer player = playerData.Player.GetModPlayer<RetroAchievementPlayer>();
            if (player.IsRaMultiPlayer)
            {
                _rejectionReason = RejectionReason.MultiPlayer;
                return false;
            }

            // Check that a multiplayer player is not being used
            if (worldFileData.TryGetHeaderData(this, out var tag) && tag.ContainsKey("RaMultiWorld"))
            {
                _rejectionReason = RejectionReason.MultiWorld;
                return false;
            }

            // Check that player was generated with this mod
            if (!player.IsRaPlayer)
            {
                _rejectionReason = RejectionReason.ExternalPlayer;
                return false;
            }

            // Check that the world was generated with this mod
            if (!WorldTool.WasGeneratedWithMod(worldFileData, "RetroAchievements"))
            {
                _rejectionReason = RejectionReason.ExternalWorld;
                return false;
            }

            // Check if any disallowed mods are loaded
            foreach (Mod mod in ModLoader.Mods)
            {
                if (!RetroAchievements.IsModAllowed(mod))
                {
                    _rejectedMod = mod.DisplayNameClean;
                    _rejectionReason = RejectionReason.ExternalMod;
                    return false;
                }
            }

            return true;
        }

        public override string WorldCanBePlayedRejectionMessage(PlayerFileData playerData, WorldFileData worldData)
        {
            switch (_rejectionReason)
            {
                case RejectionReason.JourneyMode:
                    return "RetroAchievements: Cannot play Journey Mode";

                case RejectionReason.AnniversarySeed:
                    return "RetroAchievements: Cannot play in a world using the Celebrationmk10 seed";

                case RejectionReason.ExternalMod:
                    return $"RetroAchievements: Cannot play alongside the {_rejectedMod} mod";

                case RejectionReason.ExternalPlayer:
                    return "RetroAchievements: Cannot use an external Player";

                case RejectionReason.ExternalWorld:
                    return "RetroAchievements: Cannot play in an external World";

                case RejectionReason.MultiPlayer:
                    return "RetroAchievements: Cannot use a Multiplayer Player";

                case RejectionReason.MultiWorld:
                    return "RetroAchievements: Cannot play in a Multiplayer World";

                case RejectionReason.None:
                    break;

                default:
                    return "RetroAchievements: Cannot play for an undefined reason";
            }

            return "";
        }

        public override void OnWorldLoad()
        {
            if (WorldTool.IsMultiplayer())
                _isRaMultiWorld = true;
        }

        public override void ClearWorld() => _isRaMultiWorld = false;

        public override void SaveWorldHeader(TagCompound tag)
        {
            if (IsRaMultiWorld)
                tag["RaMultiWorld"] = true;
        }

        /// <summary>
        /// Achievements.OnAchievementCompleted event callback to process in-game achievement unlocks
        /// </summary>
        /// <param name="achievement">Achievement information</param>
        private void Achievements_OnAchievementCompleted(Achievement achievement)
        {
            RetroAchievementPlayer player = Main.LocalPlayer.GetModPlayer<RetroAchievementPlayer>();
            if (player == null || !player.CanEarnAchievement())
                return;

            UnlockAchievementCommand.Invoke(this, new UnlockAchievementEventArgs(achievement.Name));
        }

        /// <summary>
        /// Main.OnAchievementCompleted event callback to disallow the selection of multiplayer
        /// </summary>
        /// <param name="gameTime">GameTime information</param>
        private void Main_OnPostDraw(GameTime gameTime)
        {
            // Only run in main menus
            if (!Main.gameMenu)
                return;

            if (Main.menuMode == MenuID.Multiplayer)
            { 
                if (RetroAchievements.IsHardcore)
                    ModContent.GetInstance<SettingsConfig>().Open(scrollToOption: "ChallengeMode");
            }
        }

        /// <summary>
        /// Detour to set a flag for players that are created with RA enabled
        /// </summary>
        /// <param name="orig">Original CreateAndSave method</param>
        /// <param name="player">Player being created</param>
        /// <returns></returns>
        private PlayerFileData On_PlayerFileData_CreateAndSave(On_PlayerFileData.orig_CreateAndSave orig, Player player)
        {
            RetroAchievementPlayer retroPlayer = player.GetModPlayer<RetroAchievementPlayer>();
            if (player != null)
                retroPlayer.Create();

            return orig.Invoke(player);
        }
    }
}
