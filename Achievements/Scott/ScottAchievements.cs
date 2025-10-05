using System.Collections.Generic;
using Terraria.Achievements;
using Terraria.ModLoader;
using TerrariaAchievementLib.Achievements.Conditions;
using RetroAchievements.Items.ScottHat;
using RetroAchievements.Paintings.Scott;

namespace RetroAchievements.Achievements.Scott
{
    public class ScottsHatAchievement : ModAchievement
    {
        public override string TextureName => "RetroAchievements/Assets/ScottsHatAchievement";

        public override void SetStaticDefaults()
        {
            Achievement.SetCategory(AchievementCategory.Collector);

            AddCondition(ItemGrabCondition.Grab(Common.reqs, ModContent.ItemType<ScottHat>()));
        }
    }

    public class ScottsPaintingAchievement : ModAchievement
    {
        public override string TextureName => "RetroAchievements/Assets/ScottsPaintingAchievement";

        public override void SetStaticDefaults()
        {
            Achievement.SetCategory(AchievementCategory.Collector);

            AddCondition(ItemGrabCondition.Grab(Common.reqs, ModContent.ItemType<ScottPaintingItem>()));
        }

        public override IEnumerable<Position> GetModdedConstraints()
        {
            yield return new After(ModContent.GetInstance<ScottsHatAchievement>());
        }
    }
}
