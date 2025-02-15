using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using RetroAchievements.Buffs;
using RetroAchievements.Systems;
using RetroAchievements.Tools;

namespace RetroAchievements.Players
{
    /// <summary>
    /// Player to save custom data and interface with the RA user
    /// </summary>
    public class RetroAchievementPlayer : ModPlayer
    {
        /// <summary>
        /// True if the player was created with this RA enabled<br/>
        /// Flag is saved when the player enters a world with nearly no play time
        /// </summary>
        private bool _isRaPlayer;

        /// <summary>
        /// True if the player was used in multiplayer with RA enabled<br/>
        /// Flag is saved when the player enters a multiplayer world
        /// </summary>
        private bool _isRaMultiPlayer;

        /// <summary>
        /// True if the player was created with RA enabled
        /// </summary>
        public bool IsRaPlayer => _isRaPlayer;

        /// <summary>
        /// True if the player was used in multiplayer with RA enabled
        /// </summary>
        public bool IsRaMultiPlayer => _isRaMultiPlayer;


        public override void OnEnterWorld()
        {
            if (WorldTool.IsMultiplayer())
                _isRaMultiPlayer = true;

            NetworkSystem network = ModContent.GetInstance<NetworkSystem>();
            if (network.IsLogin)
            {
                GiveAchievementBuff();
                MessageTool.ChatLog($"Welcome back, {network.User}!");
                MessageTool.Log($"You have earned {network.UnlockedAchs.Count}/{RetroAchievements.GetAchievementCount()} achievements for {RetroAchievements.GetGameName()}");
            }
            else
                MessageTool.ChatLog($"Login with the /ra chat command to start earning achievements!");
        }

        public override void OnRespawn()
        {
            NetworkSystem network = ModContent.GetInstance<NetworkSystem>();
            if (network.IsLogin)
                GiveAchievementBuff();
        }

        public override void SaveData(TagCompound tag)
        {
            if (IsRaPlayer)
                tag["RaPlayer"] = true;

            if (IsRaMultiPlayer)
                tag["RaMultiPlayer"] = true;
        }

        public override void LoadData(TagCompound tag)
        {
            _isRaPlayer = tag.GetBool("RaPlayer");
            _isRaMultiPlayer = tag.GetBool("RaMultiPlayer");
        }

        /// <summary>
        /// Create a player with RA enabled
        /// </summary>
        public void Create() => _isRaPlayer = true;

        /// <summary>
        /// Check if the player can earn an achievement
        /// </summary>
        /// <returns>True if the player can earn an achievement</returns>
        public bool CanEarnAchievement()
        {
            if (Player == null)
                return false;

            return Player.HasBuff(ModContent.BuffType<HardcoreAchievementBuff>()) || Player.HasBuff(ModContent.BuffType<SoftcoreAchievementBuff>());
        }

        /// <summary>
        /// Give the achievement buff to the player
        /// </summary>
        public void GiveAchievementBuff()
        {
            if (RetroAchievements.IsHardcore)
                Player.AddBuff(ModContent.BuffType<HardcoreAchievementBuff>(), 1);

            else
                Player.AddBuff(ModContent.BuffType<SoftcoreAchievementBuff>(), 1);
        }

        /// <summary>
        /// Take the achievement buff from the player
        /// </summary>
        public void TakeAchievementBuff()
        {
            if (Player.HasBuff(ModContent.BuffType<HardcoreAchievementBuff>()))
                Player.ClearBuff(ModContent.BuffType<HardcoreAchievementBuff>());
            
            if (Player.HasBuff(ModContent.BuffType<SoftcoreAchievementBuff>()))
                Player.ClearBuff(ModContent.BuffType<SoftcoreAchievementBuff>());
        } 
    }
}