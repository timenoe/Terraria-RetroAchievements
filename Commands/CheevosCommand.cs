using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAchievementLib.Tools;

namespace RetroAchievements.Commands
{
    /// <summary>
    /// Chat command to interface with local in-game achievements
    /// </summary>
    public class CheevosCommand : ModCommand
    {
        public override CommandType Type 
            => CommandType.Chat;

        public override string Command 
            => "cheevos";

        public override string Description 
            => "Manage local in-game achievements";

        public override string Usage
            => "/cheevos <command> [arguments]" +
            "\nmissing <title> - Display missing elements for a tracked achievement" +
            "\nreset all - Reset progress for all local achievements" +
            "\nreset <title> - Reset progress for a single local achievement";

        public override bool IsCaseSensitive => true;


        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                LogTool.DisplayUsage(Usage);
                return;
            }

            switch (args[0])
            {
                case "missing":
                {
                    if (args.Length < 2)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                        LogTool.ChatLog(input);
                    string localizedName = input.Split($"{args[0]} ")[1];
                    if (AchievementTool.GetMissingElementsLocalized(localizedName, out string result))
                        LogTool.ChatLog(result);
                    else
                        LogTool.ChatLog($"Failed to get missing achievement elements ({result})", ChatLogType.Error);

                    break;
                }

                case "reset":
                {
                    if (args.Length < 2)
                    {
                        LogTool.DisplayUsage(Usage);
                        return;
                    }

                    switch (args[1])
                    {
                        case "all":
                            Main.Achievements.ClearAll();
                            LogTool.ChatLog("Successfully reset all local achievement progress!", sound: SoundID.AchievementComplete);
                            break;
                        
                        default:
                            string localizedName = input.Split($"{args[0]} ")[1];
                            if (AchievementTool.ResetAchievementLocalized(localizedName, out string result))
                                LogTool.ChatLog(result, sound: SoundID.AchievementComplete);
                            else
                                LogTool.ChatLog($"Failed to reset local achievement progress ({result})", ChatLogType.Error);

                            break;
                    }

                    break;
                }

                default:
                    LogTool.DisplayUsage(Usage);
                    break;
            }
        }
    }
}
