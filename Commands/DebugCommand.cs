﻿using Terraria.ModLoader;
using TerrariaAchievementLib.Tools;

namespace RetroAchievements.Commands
{
    /// <summary>
    /// Chat command for debugging purposes
    /// </summary>
    public class DebugCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "rad";

        public override string Description
            => "Perform debugging functions";

        public override string Usage
            => "/rad <command> [arguments]" +
            "\nunlock <title> - Unlock a local achievement";

        public override bool IsCaseSensitive => true;


        public override void Action(CommandCaller caller, string input, string[] args)
        {
#if !DEBUG
            LogTool.DisplayUsage("You are not permitted to use this command");
            return;
#endif
            if (args.Length == 0)
            {
                LogTool.DisplayUsage(Usage);
                return;
            }

            switch (args[0])
            {
                case "unlock":
                    if (args.Length < 2)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                    switch (args[1])
                    {
                        // TODO: Add option to unlock all achievements
                        case "all":
                            LogTool.DisplayUsage(Usage);
                            break;

                        case "progression":
                        {
                            string[] progressionAchNames = ["Boned", "Still Hungry", "Buckets of Bolts", "The Great Southern Plantkill", "Lihzahrdian Idol", "Obsessive Devotion", "Star Destroyer", "Champion of Terraria"];
                            foreach (string progressionAchName in progressionAchNames)
                            {
                                if (!AchievementTool.UnlockAchievementLocalized(progressionAchName, out string result))
                                    LogTool.ChatLog($"Failed to unlock local achievement ({result})", ChatLogType.Error);
                            }
                            break;
                        }

                        default:
                        {
                            string localizedName = input.Split($"{args[0]} ")[1];
                            if (!AchievementTool.UnlockAchievementLocalized(localizedName, out string result))
                                LogTool.ChatLog($"Failed to unlock local achievement ({result})", ChatLogType.Error);

                            break;
                        }
                    }

                    break;

                default:
                    LogTool.DisplayUsage(Usage);
                    break;
            }
        }
    }
}
