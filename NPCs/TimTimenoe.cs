using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.NPCs
{
    /// <summary>
    /// Rarely changes Tim's name to Timenoe
    /// </summary>
    public class TimTimenoe : GlobalNPC
    {
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type == NPCID.Tim)
            {
                // 1% chance to set name to Timenoe
                Random random = new();
                int rng = random.Next(100);
                if (rng == 0)
                    npc.GivenName = "Timenoe";
            }
        }
    }
}
