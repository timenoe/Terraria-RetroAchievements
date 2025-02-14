using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RetroAchievements.Pets
{
    /// <summary>
    /// Allows Catto to follow the player
    /// </summary>
    public class CattoProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            // Mimic Black Cat
            Projectile.CloneDefaults(ProjectileID.BlackCat); 
            AIType = ProjectileID.BlackCat;

            // Prevent from being drawn in ground
            DrawOriginOffsetY = -10;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            // Relic from AIType
            player.blackCat = false; 

            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff
            if (!player.dead && player.HasBuff(ModContent.BuffType<CattoBuff>()))
            {
                Projectile.timeLeft = 2;
            }
        }
    }
}
