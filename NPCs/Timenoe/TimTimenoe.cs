using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using RetroAchievements.Pets.Catto;

namespace RetroAchievements.NPCs.Timenoe
{
    /// <summary>
    /// Rarely changes Tim's name to Timenoe
    /// </summary>
    public class TimTimenoe : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.Tim)
            {
                ItemDropWithConditionRule rule = new(ModContent.ItemType<CattoItem>(), 1, 1, 1, new Conditions.NamedNPC("Timenoe"));
                npcLoot.Add(rule);
            }
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type == NPCID.Tim)
            {
                // 1% chance to set name to Timenoe
                Random random = new();
                int rng = random.Next(100);
                if (rng == 7)
                    npc.GivenName = "Timenoe";
            }
        }
    }
}
