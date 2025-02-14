using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Tiles
{
    /// <summary>
    /// Prevents the Bast Statue from being toggled to prevent the Transmutation Glitch
    /// </summary>
    public class FixedBastTile : GlobalTile
    {
        public override bool PreHitWire(int i, int j, int type) => !(type == TileID.CatBast);
    }
}
