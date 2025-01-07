﻿using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace RetroAchievements.Configs
{
    /// <summary>
    /// Config file for RA settings
    /// </summary>
    public class SettingsConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        /// <summary>
        /// RetroAchievements host to send requests to
        /// </summary>
        [BackgroundColor(0, 0, 255)]
        [DefaultValue("stage.retroachievements.org")]
        [ReloadRequired]
        public string Host;

        /// <summary>
        /// Game to load achievements for
        /// </summary>
        [BackgroundColor(0, 255, 0)]
        [DefaultValue(AchievementGame.Vanilla)]
        [DrawTicks]
        [ReloadRequired]
        [SliderColor(0, 255, 0)]
        public AchievementGame Game;

        /// <summary>
        /// True if RA hardcore is enabled
        /// </summary>
        [BackgroundColor(255, 255, 0)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool Hardcore;
    }
}
