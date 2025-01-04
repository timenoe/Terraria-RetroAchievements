using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Achievements;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using RetroAchievementsMod.Players;
using RetroAchievementsMod.Utils;

namespace RetroAchievementsMod.Systems
{
    public class AchievementSystem : ModSystem
    {
        public enum RejectionReason { NONE = -1, JOURNEY, MODS, MULTI};
        public string rejectedMod = "";

        public bool hasSelectedMulti = false;
        public RejectionReason rejectionReason = RejectionReason.NONE;

        private void Achievements_OnAchievementCompleted(Achievement achievement)
        {
            AchievementPlayer player = Main.LocalPlayer.GetModPlayer<AchievementPlayer>();
            if (player == null || !player.canEarnAchievements)
                return;
            
            MessageUtil.Display($"Queued unlock for [a:{achievement.Name}]");

            // TODO: Queue RA HTTP request
        }

        // Called after Main draws
        private void Main_OnPostDraw(GameTime obj)
        {
            // Only run in title menus
            if (Main.gameMenu)
            {
                if (Main.menuMode == MenuID.Multiplayer)
                    hasSelectedMulti = true;

                else if (Main.menuMode == MenuID.Title)
                    hasSelectedMulti = false;
            }
        }

        // Called when the mod loads
        public override void OnModLoad()
        {
            Main.Achievements.OnAchievementCompleted += Achievements_OnAchievementCompleted;
            Main.OnPostDraw += Main_OnPostDraw;
        }

        public override void OnModUnload()
        {
            Main.Achievements.OnAchievementCompleted -= Achievements_OnAchievementCompleted;
            Main.OnPostDraw -= Main_OnPostDraw;
        }

        // Called when the user selects a world to play
        public override bool CanWorldBePlayed(PlayerFileData playerData, WorldFileData worldFileData)
        {
            // Ensure Player is not Journey Mode
            // Vanilla already ensures Journey worlds must use Journey players
            if (playerData.Player.difficulty == PlayerDifficultyID.Creative)
            { 
                rejectionReason = RejectionReason.JOURNEY;
                return false;
            }

            // Check if any non-permitted mods are loaded
            foreach (Mod mod in ModLoader.Mods)
            {
                if (mod != null && !RetroAchievementsMod.IsModAllowed(mod))
                {
                    rejectedMod = mod.DisplayNameClean;
                    rejectionReason = RejectionReason.MODS;
                    return false;
                }  
            }

            // Check if Multiplayer > Host & Play was selected
            // Join via IP/Steam does not select a world
            // Checked for in AchievementPlayer.OnEnterWorld
            if (!hasSelectedMulti && !RetroAchievementsMod.IsMultiAllowed())
            {
                rejectionReason = RejectionReason.MULTI;
                return false;
            }

            rejectionReason = RejectionReason.NONE;
            return true;
        }

        // Called when displaying the rejection message after CanWorldBePlayed fails
        public override string WorldCanBePlayedRejectionMessage(PlayerFileData playerData, WorldFileData worldData)
        {
            switch (rejectionReason)
            { 
                case RejectionReason.JOURNEY:
                    return "RetroAchievements Mod: Cannot play Journey Mode";

                case RejectionReason.MODS:
                    return $"RetroAchievements Mod: Cannot play with {rejectedMod}";

                case RejectionReason.MULTI:
                    return "RetroAchievements Mod: Cannot play Multiplayer";

                default:
                    return "RetroAchievements Mod: Cannot play for undefined reason";
            }
        }
    }
}
