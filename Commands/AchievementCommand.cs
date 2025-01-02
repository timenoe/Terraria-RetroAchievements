using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using RetroAchievementsMod.Utils;

namespace RetroAchievementsMod.Commands
{
    public class AchievementCommand : ModCommand
    {
        // Command can be used in Chat in SP and MP
        public override CommandType Type => CommandType.Chat;

        // Desired text to trigger this command
        public override string Command => "cheevos";

        // Short description of this command
        public override string Description => "Manage local in-game achievements";

        // Short usage explanation of this command
        public override string Usage => "/cheevos reset all";

        // Called when the command is entered into chat
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                DisplayUsage();
                return;
            }

            switch (args[0])
            {
                case "reset":
                    if (args.Length == 1)
                    {
                        DisplayUsage();
                        return;
                    }
                    switch (args[1])
                    {
                        case "all":
                            Main.Achievements.ClearAll();
                            MessageUtil.Display("Successfully reset all local in-game achievements", MessageType.SUCCESS, SoundID.AchievementComplete);
                            break;

                        default:
                            DisplayUsage();
                            break;
                    }
                    break;

                default:
                    DisplayUsage();
                    break;
            }
        }

        // Displays the proper usage of the command
        private void DisplayUsage()
        {
            MessageUtil.Display($"Usage: {Usage}", MessageType.ERROR, SoundID.PlayerHit);
        }
    }
}
