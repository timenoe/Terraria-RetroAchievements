using Terraria;
using Terraria.ModLoader;

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

            if (chatText.StartsWith("/ra login "))
            {
                string[] chatWords = chatText.Split("/ra login ");
                int indexOfSpace = chatWords[1].IndexOf(" ");
                    string password = chatWords[1].Substring(indexOfSpace + 1);
                    string hiddenPass = new('*', password.Length);

                    Main.chatText = chatText.Replace(password, hiddenPass);
            }

            //if (chatWords.Length >= 4 && chatWords[0] == "/ra" && chatWords[1] == "login")
            //{
            //    // Hide password
            //    if (!string.IsNullOrEmpty(chatWords[3]))
            //    {
            //        chatWords[3] = new('*', chatWords[3].Length);
            //        Main.chatText = string.Join(" ", chatWords);
            //    }
            //}

            // Draw chat text with hidden password
            orig.Invoke(self);

            // Restore original chat text
            Main.chatText = chatText;
        }
    }
}
