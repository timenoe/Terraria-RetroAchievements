using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievementsMod.Buffs
{
    public class AchievementBuff : ModBuff
    {
        // Make buff duration infinite
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        }
        
        // Set buff name and tooltip
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            buffName = "Retro Gamer";
            tip = "RetroAchievements are enabled";
        }

        // Ignore right clicks (which would remove the buff)
        public override bool RightClick(int buffIndex)
        {
            return false;
        }
    }
}