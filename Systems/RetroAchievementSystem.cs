using Terraria;
using TerrariaAchievementLib.Systems;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Creates a helper AchievementSystem instance
    /// </summary>
    public class RetroAchievementSystem : AchievementSystem
    {
         public override void OnModLoad()
        {
            if (Main.dedServ)
                return;

            base.OnModLoad();
            ProgressSystem.SetEnabled(true, includeVanilla: true);
        }
    }
}