using System.Collections.Generic;
using Terraria.Achievements;
using Terraria.ModLoader;
using TerrariaAchievementLib.Systems;
using TerrariaAchievementLib.Achievements;
using TerrariaAchievementLib.Achievements.Conditions;
using RetroAchievements.Items;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Adds new achievements to the in-game list
    /// </summary>
    public class RetroAchievementSystem : AchievementSystem
    {
        protected override string Identifier { get => "RETRO"; }

        protected override List<string> TexturePaths { get => ["RetroAchievements/Assets/Achievements"]; }


        protected override void RegisterAchievements()
        {
            ConditionReqs reqs = new(PlayerDiff.Classic, WorldDiff.Classic, SpecialSeed.None);

            RegisterAchievement("SCOTTS_HAT", ItemGrabCondition.Grab(reqs, ModContent.ItemType<ScottsHat>()), AchievementCategory.Collector);
        }
    }
}