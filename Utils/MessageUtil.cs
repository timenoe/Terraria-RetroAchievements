using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;

namespace RetroAchievementsMod.Utils
{
    public enum MessageType { INFO, SUCCESS, WARNING, ERROR };

    public class MessageUtil
    {
        // RA stylized message header
        private const string messageHeader = "[c/327DFF:Retro][c/FFF014:Achievements] [c/FFFFFF:Mod:] ";

        // Displays a stylized message in-game
        public static void Display(string msg, MessageType msgType = MessageType.INFO, SoundStyle msgSound = default)
        {
            switch (msgType)
            {
                case MessageType.INFO:
                    Main.NewText($"{messageHeader} {msg}");
                    break;

                case MessageType.SUCCESS:
                    Main.NewText($"{messageHeader} {msg}", Color.Green);
                    break;

                case MessageType.WARNING:
                    Main.NewText($"{messageHeader} {msg}", Color.Yellow);
                    break;

                case MessageType.ERROR:
                    Main.NewText($"{messageHeader} {msg}", Color.Red);
                    break;
            }

            if (msgSound != default)
                SoundEngine.PlaySound(msgSound);
        }
    }
}
