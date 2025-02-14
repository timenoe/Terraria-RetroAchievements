using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Items
{
    /// <summary>
    /// Scott's Hat
    /// </summary>
    [AutoloadEquip(EquipType.Head)]
    public class ScottHat : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 10;

            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }
    }
}
