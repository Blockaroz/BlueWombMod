using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss.Projectiles;

public sealed class BloodTear : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;

        Projectile.friendly = false;
        Projectile.hostile = true;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float Mode => ref Projectile.ai[1];

    public ref float Weight => ref Projectile.ai[2];

    public ref float MiscTime => ref Projectile.localAI[0];

    public override void AI()
    {
        if (MiscTime == 0)
        {
            MiscTime = Main.rand.Next(30);
            Projectile.scale *= Main.rand.NextFloat(0.6f, 1.2f) + Weight * 0.1f;
        }

        switch (Mode)
        {
            default:
            case 0: // Fly forward

                Projectile.velocity *= 0.98f;

                break;

            case 1: // Fall and shrink

                Projectile.velocity.Y += 0.2f * Weight;

                break;

            case 2: // Large and split on death

                if (Time == 0)
                {
                    Projectile.Resize(24, 24);
                }

                break;
        }

        Dust dust1 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * Projectile.scale, DustID.Blood, Projectile.velocity * 0.5f, Scale: Main.rand.NextFloat(0.5f, 1.5f));
        dust1.noGravity = true;

        Projectile.frame = (int)(MiscTime / 5) % 2;

        Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);

        Time++;
        MiscTime++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        float scale = Utils.GetLerpValue(-2, 6, MiscTime, true);

        Texture2D glow = Assets.Textures.GlowBig.Value;
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.White with { A = 0 } * 0.1f, Projectile.rotation, glow.Size() / 2, Projectile.scale * scale * 0.15f, 0, 0);

        Rectangle frame = texture.Frame(1, 2, 0, Projectile.frame);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        return false;
    }
}
