﻿using System;
using Terraria.ModLoader;
using RetroAchievements.Systems;
using RetroAchievements.Utils;

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
            "\nhost - Get the current host" +
            "\nlogin <user> <pass> - Login a user" +
            "\nrp - Get the current Rich Presence";

        /// <summary>
        /// Event to login a user
        /// </summary>
        public event EventHandler<LoginEventArgs> LoginCommand;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                MessageUtil.DisplayUsage(Usage);
                return;
            }

            switch (args[0])
            {
                case "host":
                    if (args.Length != 1)
                    {
                        MessageUtil.DisplayUsage(Usage);
                        return;
                    }

                    MessageUtil.ChatLog($"The current host is {RetroAchievements.Host}");
                    break;

                case "login":
                    if (args.Length != 3)
                    {
                        MessageUtil.DisplayUsage(Usage);
                        return;
                    }

                    LoginCommand?.Invoke(this, new LoginEventArgs(args[1], args[2]));
                    break;

                case "rp":
                    if (args.Length != 1)
                    {
                        MessageUtil.DisplayUsage(Usage);
                        return;
                    }

                    MessageUtil.ChatLog($"Current Rich Presence\n{RichPresenceSystem.GetRichPresence().Replace("• ", "\n")}");
                    break;

                default:
                    MessageUtil.DisplayUsage(Usage);
                    break;
            }
        }
    }
}
