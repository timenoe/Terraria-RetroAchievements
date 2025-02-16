using Terraria.ModLoader;
using TerrariaAchievementLib.Systems;
using RetroAchievements.Tools;

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
            "\nunlock <achievement_title> - Unlock a single local achievement";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
#if !DEBUG
            MessageTool.DisplayUsage("You are not permitted to use this command");
            return;
#endif
            if (args.Length == 0)
            {
                MessageTool.DisplayUsage(Usage);
                return;
            }

            switch (args[0])
            {
                case "unlock":
                    if (args.Length < 2)
                    {
                        MessageTool.DisplayUsage(Usage);
                        return;
                    }

                    switch (args[1])
                    {
                        // TODO: Add option to unlock all achievements
                        case "all":
                            MessageTool.DisplayUsage(Usage);
                            break;

                        default:
                            string displayName = input.Split($"{args[0]} ")[1];
                            string internalName = RetroAchievements.GetAchievementInternalName(displayName);

                            if (string.IsNullOrEmpty(internalName))
                            {
                                MessageTool.ChatLog($"The achievement name \"{displayName}\" is not recognized", ChatLogType.Error);
                                break;
                            }

                            if (!AchievementSystem.UnlockIndividualAchievement(internalName))
                                MessageTool.ChatLog($"Failed to unlock [a:{internalName}]", ChatLogType.Error);

                            break;
                    }

                    break;

                default:
                    MessageTool.DisplayUsage(Usage);
                    break;
            }
        }
    }
}
