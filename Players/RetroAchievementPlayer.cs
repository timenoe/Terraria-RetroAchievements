using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using RetroAchievements.Buffs;
using RetroAchievements.Systems;
using RetroAchievements.Utils;

namespace RetroAchievements.Players
{
    /// <summary>
    /// Player to save custom data and interface with the RA user
    /// </summary>
    public class RetroAchievementPlayer : ModPlayer
    {
        /// <summary>
        /// True if the player can earn achievements<br/>
        /// Set after entering the world and passing any final checks
        /// </summary>
        private bool _canEarnAchievements;

        /// <summary>
        /// True if the player was created with this mod<br/>
        /// Flag is saved when the player enters a world with no play time
        /// </summary>
        private bool _wasCreatedWithRa;


        /// <summary>
        /// True if the player can earn achievements
        /// </summary>
        public bool CanEarnAchievements => _canEarnAchievements;

        /// <summary>
        /// True if the player was created with this mod
        /// </summary>
        public bool WasCreatedWithRa => _wasCreatedWithRa;


        public override void OnEnterWorld()
        {
            if (!RetroAchievements.IsEnabled)
                return;

            if (!RetroAchievements.IsSinglePlayer() && !RetroAchievements.IsMultiAllowed())
            {
                MessageUtil.ChatLog("Cannot play Multiplayer", ChatLogType.Error);
                return;
            }

            // Set custom flag if the player was created while this mod was active
            if (PlayerUtil.HasNoPlayTime())
                _wasCreatedWithRa = true;

            NetworkSystem network = ModContent.GetInstance<NetworkSystem>();
            if (network.IsStarted)
            {
                GrantAchievementBuff();
                MessageUtil.ChatLog($"Welcome back, {network.User}!");
            }
            else
                MessageUtil.ChatLog($"Login with the /ra chat command to start earning achievements!");
        }

        public override void SaveData(TagCompound tag)
        {
            if (WasCreatedWithRa)
                tag["WasCreatedWithRa"] = true;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("WasCreatedWithRa"))
                _wasCreatedWithRa = true;
        }

        /// <summary>
        /// Grant the achievement buff to the player
        /// </summary>
        public void GrantAchievementBuff()
        {
            Player.AddBuff(ModContent.BuffType<AchievementBuff>(), 1);
            _canEarnAchievements = true;
        }
    }
}