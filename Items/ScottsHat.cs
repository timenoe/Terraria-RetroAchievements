using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Items
{
    /// <summary>
    /// Hat that drops from Scott
    /// </summary>
    [AutoloadEquip(EquipType.Head)]
    public class ScottsHat : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;

            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }
    }
}
