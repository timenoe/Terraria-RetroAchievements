using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using TerrariaAchievementLib.Tools;
using RetroAchievements.Systems;

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
            "\nrp - Get the current RA Rich Presence" +
            "\nsync - Unlock local achievements that are already unlocked on RA";

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
                LogTool.DisplayUsage(Usage);
                return;
            }

            NetworkSystem network = ModContent.GetInstance<NetworkSystem>();

            switch (args[0])
            {
                case "host":
                    if (args.Length != 1)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                    LogTool.ChatLog($"The current RA host is {RetroAchievements.Host}");
                    break;

                case "login":
                    if (args.Length < 3)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                    // Support passwords with spaces
                    string pass = input.Split($"{args[1]} ")[1];

                    LoginCommand?.Invoke(this, new LoginEventArgs(args[1], pass));
                    break;

                case "logout":
                    if (args.Length != 1)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                    if (!network.IsLogin)
                    {
                        LogTool.ChatLog("You are not logged into RA", ChatLogType.Error);
                        return;
                    }

                    LogoutCommand?.Invoke(this, EventArgs.Empty);
                    break;

                case "rp":
                    if (args.Length != 1)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                    LogTool.ChatLog($"Current Rich Presence\n{RichPresenceSystem.GetRichPresence().Replace("• ", "\n")}");
                    break;

                case "sync":
                    if (args.Length != 1)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                    List<int> achs = RetroAchievements.IsHardcore ? network.UnlockedHardcoreAchs : network.UnlockedAchs;
                    foreach (var id in achs)
                    {
                        string internalName = RetroAchievements.GetAchievementInternalName(id);
                        if (string.IsNullOrEmpty(internalName))
                            continue;

                        AchievementTool.UnlockAchievementInternal(internalName, out _);
                    }
                    
                    break;

                default:
                    LogTool.DisplayUsage(Usage);
                    break;
            }
        }
    }
}
