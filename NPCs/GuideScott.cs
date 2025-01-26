using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using RetroAchievements.Items;

namespace RetroAchievements.NPCs
{
    /// <summary>
    /// RetroAchievements founder Scott<br/>
    /// He appears when the Guide's name is Scott<br/>
    /// He drops his signature hat when killed<br/>
    /// 1.1.1.1215208201
    /// </summary>
    public class GuideScott : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.Guide)
            {
                ItemDropWithConditionRule rule = new(ModContent.ItemType<ScottsHat>(), 1, 1, 1, new Conditions.NamedNPC("Scott"));
                npcLoot.Add(rule);
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type == NPCID.Guide && npc.GivenName == "Scott")
                TextureAssets.Npc[NPCID.Guide] = ModContent.Request<Texture2D>("RetroAchievements/NPCs/GuideScott");

            return true;
        }

        public override void GetChat(NPC npc, ref string chat)
        {
            if (npc.type == NPCID.Guide && npc.GivenName == "Scott")
                chat = "Welcome to RA! Hope you're having fun!!!!";
        }
    }
}
