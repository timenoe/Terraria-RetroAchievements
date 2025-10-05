using TerrariaAchievementLib.Achievements;

namespace RetroAchievements.Achievements
{
    /// <summary>
    /// Commonly shared between achievements
    /// </summary>
    public class Common
    {
        /// <summary>
        /// Achievement condition requirements
        /// </summary>
        public static readonly ConditionReqs reqs = new(PlayerDiff.Classic, WorldDiff.Classic, SpecialSeed.None);
    }
}
