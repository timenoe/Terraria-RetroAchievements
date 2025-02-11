using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Items
{
    /// <summary>
    /// Scott painting item
    /// </summary>
    public class ScottPainting : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ScottPaintingTile>());
            
            Item.width = 16;
            Item.height = 24;
            Item.rare = ItemRarityID.Yellow;
        }
    }
}
