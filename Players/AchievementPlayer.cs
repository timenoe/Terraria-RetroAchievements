using Terraria.ModLoader;
using RetroAchievementsMod.Buffs;
using RetroAchievementsMod.Utils;
using Terraria;
using Terraria.ID;

namespace RetroAchievementsMod.Players
{
    public class AchievementPlayer : ModPlayer
    {
        // True if the player can earn achievements
        // Also ensures the player has entered the world
        public bool canEarnAchievements = false;
        
        // Called when the player enters a world
        // Checks if achievements can be enabled
        // AchievementSystem.CanWorldBePlayed does pre-checks
        public override void OnEnterWorld()
        {
            canEarnAchievements = false;
            Player.ClearBuff(ModContent.BuffType<AchievementBuff>());

            if (Main.netMode != NetmodeID.SinglePlayer && !RetroAchievementsMod.IsMultiAllowed())
            {
                MessageUtil.Display("Cannot play Multiplayer", MessageType.ERROR);
                return;
            }

            canEarnAchievements = true;
            Player.AddBuff(ModContent.BuffType<AchievementBuff>(), 1);
            MessageUtil.Display($"Track progress at retroachievements.org!");
        }
    }
}