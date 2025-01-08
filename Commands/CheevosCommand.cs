using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using RetroAchievements.Utils;

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
            "\nreset <all> - Reset all local in-game achievements";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                MessageUtil.DisplayUsage(Usage);
                return;
            }

            switch (args[0])
            {
                case "reset":
                    if (args.Length != 2)
                    {
                        MessageUtil.DisplayUsage(Usage);
                        return;
                    }
                    switch (args[1])
                    {
                        case "all":
                            Main.Achievements.ClearAll();
                            MessageUtil.ChatLog("Successfully reset all local in-game achievements", sound: SoundID.AchievementComplete);
                            break;

                        // TODO: Add options for resetting individual achievements, achievement categories, etc.
                        default:
                            MessageUtil.DisplayUsage(Usage);
                            break;
                    }
                    break;

                default:
                    MessageUtil.DisplayUsage(Usage);
                    break;
            }
        }
    }
}
