using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Paintings.Scott
{
    /// <summary>
    /// Scott painting item
    /// </summary>
    public class ScottPaintingItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ScottPaintingTile>());

            Item.width = 16;
            Item.height = 24;
            Item.rare = ItemRarityID.Yellow;
        }
    }
}
