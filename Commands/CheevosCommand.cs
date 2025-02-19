using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAchievementLib.Systems;
using RetroAchievements.Tools;

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
            "\nreset all - Reset progress for all local achievements" +
            "\nreset <achievement_title> - Reset progress for a single local achievement";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                MessageTool.DisplayUsage(Usage);
                return;
            }

            switch (args[0])
            {
                case "reset":
                    if (args.Length < 2)
                    {
                        MessageTool.DisplayUsage(Usage);
                        return;
                    }

                    switch (args[1])
                    {
                        case "all":
                            Main.Achievements.ClearAll();
                            MessageTool.ChatLog("Successfully reset progress for all local achievements!", sound: SoundID.AchievementComplete);
                            break;

                        default:
                            string displayName = input.Split($"{args[0]} ")[1];
                            string internalName = RetroAchievements.GetAchievementInternalName(displayName);

                            if (string.IsNullOrEmpty(internalName))
                            {
                                MessageTool.ChatLog($"The achievement name \"{displayName}\" is not recognized", ChatLogType.Error);
                                break;
                            }

                            if (AchievementSystem.ResetInvidualAchievement(internalName))
                                MessageTool.ChatLog($"Successfully reset local progress for [a:{internalName}]!", sound: SoundID.AchievementComplete);
                            else
                                MessageTool.ChatLog($"Failed to reset local progress for [a:{internalName}]", ChatLogType.Error);
                            
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
