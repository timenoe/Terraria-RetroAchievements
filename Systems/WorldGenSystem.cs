using Terraria.GameContent.Generation;
using Terraria.ModLoader;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Places items during world generation
    /// </summary>
    public class WorldGenSystem : ModSystem
    {
        /// <summary>
        /// True if the Scott painting has already been generated
        /// </summary>
        private bool _createdScottPainting;

        public override void OnModLoad() => Terraria.On_WorldGen.RandHousePicture += On_WorldGen_RandHousePicture;

        public override void OnModUnload() => Terraria.On_WorldGen.RandHousePicture -= On_WorldGen_RandHousePicture;

        public override void PreWorldGen() => _createdScottPainting = false;

        /// <summary>
        /// Detour to place the Scott painting during world generation
        /// </summary>
        /// <param name="orig">Original RandHousePicture method</param>
        /// <returns>Painting to place in a house</returns>
        private PaintingEntry On_WorldGen_RandHousePicture(Terraria.On_WorldGen.orig_RandHousePicture orig)
        {
            if (!_createdScottPainting)
            {
                PaintingEntry result = default;
                result.tileType = ModContent.TileType<Tiles.ScottPaintingTile>();
                _createdScottPainting = true;
                return result;
            }

            return orig.Invoke();
        }
    }
}
