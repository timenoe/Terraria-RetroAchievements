using Terraria;
using Terraria.ModLoader;
using TerrariaAchievementLib.Tools;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Used to modify chat menu text
    /// </summary>
    public class ChatSystem : ModSystem
    {
        public override void OnModLoad()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawPlayerChat += On_Main_DrawPlayerChat;
        }

        public override void OnModUnload()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawPlayerChat -= On_Main_DrawPlayerChat;
        } 

        /// <summary>
        /// Detour to hide passwords in the chat menu<br/>
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void On_Main_DrawPlayerChat(On_Main.orig_DrawPlayerChat orig, Main self)
        {
            string chatText = Main.chatText;

            // Hide password while typing if applicable
            if (chatText.StartsWith("/ra login "))
            {
                string[] loginText = chatText.Split("/ra login ");
                if (loginText.Length >= 2)
                {
                    string[] userPass = loginText[1].Split(" ", 2);
                    if (userPass.Length >= 2)
                    {
                        string user = userPass[0];
                        string pass = userPass[1];
                        LogTool.ModLog(pass);
                        if (!string.IsNullOrEmpty(pass))
                        {
                            string hiddenPass = new('*', pass.Length);
                            Main.chatText = "/ra login " + user + " " + hiddenPass;
                        }
                    }
                }
            }

            // Draw chat text with potentially hidden password
            orig.Invoke(self);

            // Restore original chat text for commands
            Main.chatText = chatText;
        }
    }
}
