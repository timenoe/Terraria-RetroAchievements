using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace RetroAchievements.Menus
{
    /// <summary>
    /// Adds the RetroAchievements logo to the main menu
    /// </summary>
    public class RetroAchievementsMenu : ModMenu
    {
        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("RetroAchievements/Menus/RetroAchievementsMenu");
    }
}
