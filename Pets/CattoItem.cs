using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Pets
{
    /// <summary>
    /// Allows Catto to be summoned
    /// </summary>
    public class CattoItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.UnluckyYarn);

            Item.buffType = ModContent.BuffType<CattoBuff>();
            Item.shoot = ModContent.ProjectileType<CattoProjectile>();
            Item.rare = ItemRarityID.Cyan;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.AddBuff(Item.buffType, 3600);

            return true;
        }
    }
}
