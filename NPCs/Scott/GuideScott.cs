using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using RetroAchievements.Paintings.Scott;
using RetroAchievements.Items.ScottHat;

namespace RetroAchievements.NPCs.Scott
{
    /// <summary>
    /// RetroAchievements founder Scott<br/>
    /// He appears when the Guide's name is Scott<br/>
    /// He drops his signature hat when given his painting or when killed<br/>
    /// 1.1.1.1215208201
    /// </summary>
    public class GuideScott : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.Guide)
            {
                ItemDropWithConditionRule rule = new(ModContent.ItemType<ScottHat>(), 1, 1, 1, new Conditions.NamedNPC("Scott"));
                npcLoot.Add(rule);
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (IsScott(npc))
                TextureAssets.Npc[NPCID.Guide] = ModContent.Request<Texture2D>("RetroAchievements/NPCs/GuideScott");

            return true;
        }

        public override void GetChat(NPC npc, ref string chat)
        {
            if (IsScott(npc))
            {
                Player player = Main.LocalPlayer;
                if (npc.FindClosestPlayer() == player.whoAmI && player.HeldItem.type == ModContent.ItemType<ScottPaintingItem>())
                {
                    chat = "You found a painting of me! Here, take this. I have an extra hat just for you.";

                    player.HeldItem.stack--;
                    Item.NewItem(new EntitySource_Gift(npc), player.Center, ModContent.ItemType<ScottHat>());
                }

                else
                    chat = "Welcome to RA! Hope you're having fun!!!!";
            }
        }

        /// <summary>
        /// Checks if a given NPC is a Guide named Scott
        /// </summary>
        /// <param name="npc">NPC to check</param>
        /// <returns>True if the NPC is a Guide named Scott</returns>
        private static bool IsScott(NPC npc) => npc.type == NPCID.Guide && npc.GivenName == "Scott";
    }
}
