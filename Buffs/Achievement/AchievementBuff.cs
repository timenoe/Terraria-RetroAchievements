using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Buffs.Achievement
{
    /// <summary>
    /// Buff to apply to the player when Hardcore RA achievements are enabled
    /// </summary>
    public class HardcoreAchievementBuff : ModBuff
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

    /// <summary>
    /// Buff to apply to the player when Softcore RA achievements are enabled
    /// </summary>
    public class SoftcoreAchievementBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        }

        public override bool RightClick(int buffIndex) => false;
    }
}