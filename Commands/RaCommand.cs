using System;
using Terraria.ModLoader;
using RetroAchievements.Systems;
using RetroAchievements.Tools;

namespace RetroAchievements.Commands
{
    /// <summary>
    /// Chat command to interface with RA
    /// </summary>
    public class RaCommand : ModCommand
    {
        public override CommandType Type 
            => CommandType.Chat;

        public override string Command 
            => "ra";

        public override string Description 
            => "Communicate with the RetroAchievements server";

        public override string Usage
            => "/ra <command> [arguments]" +
            "\nhost - Get the current RA host" +
            "\nlogin <user> <pass> - Login an RA user" +
            "\nlogout <user> <pass> - Logout an RA user" +
            "\nrp - Get the current RA Rich Presence";

        /// <summary>
        /// Event to login a user
        /// </summary>
        public event EventHandler<LoginEventArgs> LoginCommand;

        /// <summary>
        /// Event to logout a user
        /// </summary>
        public event EventHandler<EventArgs> LogoutCommand;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                MessageTool.DisplayUsage(Usage);
                return;
            }

            switch (args[0])
            {
                case "host":
                    if (args.Length != 1)
                    {
                        MessageTool.DisplayUsage(Usage);
                        return;
                    }

                    MessageTool.ChatLog($"The current RA host is {RetroAchievements.Host}");
                    break;

                case "login":
                    if (args.Length != 3)
                    {
                        MessageTool.DisplayUsage(Usage);
                        return;
                    }

                    LoginCommand?.Invoke(this, new LoginEventArgs(args[1], args[2]));
                    break;

                case "logout":
                    if (args.Length != 1)
                    {
                        MessageTool.DisplayUsage(Usage);
                        return;
                    }

                    NetworkSystem network = ModContent.GetInstance<NetworkSystem>();
                    if (!network.IsLogin)
                    {
                        MessageTool.ChatLog("You are not logged into RA", ChatLogType.Error);
                        return;
                    }

                    LogoutCommand?.Invoke(this, EventArgs.Empty);
                    break;

                case "rp":
                    if (args.Length != 1)
                    {
                        MessageTool.DisplayUsage(Usage);
                        return;
                    }

                    MessageTool.ChatLog($"Current Rich Presence\n{RichPresenceSystem.GetRichPresence().Replace("• ", "\n")}");
                    break;

                default:
                    MessageTool.DisplayUsage(Usage);
                    break;
            }
        }
    }
}
