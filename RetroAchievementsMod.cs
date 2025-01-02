using System.Linq;
using Terraria.ModLoader;

namespace RetroAchievementsMod
{
    public class RetroAchievementsMod : Mod
	{
        // Returns true if a specific mod is permitted
        public static bool IsModAllowed(Mod mod)
        {
            string[] allowedMods = ["ModLoader", "RetroAchievementsMod"];
            return allowedMods.Contains(mod.Name);
        }

        // Returns true if Multiplayer is allowed
        public static bool IsMultiAllowed()
        {
            return false;
        }
	}
}
