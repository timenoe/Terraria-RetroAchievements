using Terraria.GameContent.Generation;
using Terraria.ModLoader;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Places items during world generation
    /// </summary>
    public class WorldGenSystem : ModSystem
    {

        private bool _createdScottPainting;

        public override void OnModLoad() => Terraria.On_WorldGen.RandHousePicture += On_WorldGen_RandHousePicture;

        public override void OnModUnload() => Terraria.On_WorldGen.RandHousePicture -= On_WorldGen_RandHousePicture;

        public override void PreWorldGen() => _createdScottPainting = false;

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
