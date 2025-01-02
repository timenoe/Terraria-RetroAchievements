using Terraria.ModLoader.Config;

namespace RetroAchievementsMod.Configs
{
    public class LoginConfig : ModConfig
    {
        // Login info is stored client-side
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // Reload is needed to login
        [ReloadRequired]
        public string UserName;
        public string Password;

        public override void OnLoaded()
        {
            // TODO: Send RA login request
        }
    }
}
