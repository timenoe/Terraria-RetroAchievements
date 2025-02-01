using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Buffs
{
    /// <summary>
    /// Buff to apply to the player when RA achievements are enabled
    /// </summary>
    public class AchievementBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // Don't save buff
            Main.buffNoSave[Type] = true;

            // Make buff duration infinite
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        }

        // Ignore right-clicks to cancel
        public override bool RightClick(int buffIndex) => false;
    }
}