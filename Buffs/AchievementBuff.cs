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

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            // Set buff name and tooltip
            buffName = "Retro Gamer";
            tip = "RetroAchievements are enabled";
        }

        public override bool RightClick(int buffIndex)
        {
            // Ignore right click (which would remove the buff)
            return false;
        }
    }
}