using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss.Projectiles;

public sealed class HolyWaterTear : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;

        Projectile.friendly = false;
        Projectile.hostile = true;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 100;
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float Mode => ref Projectile.ai[1];

    public ref float Curvature => ref Projectile.ai[2];

    public ref float MiscTime => ref Projectile.localAI[0];

    public override void AI()
    {
        if (MiscTime == 0)
        {
            MiscTime = Main.rand.Next(30);
            Projectile.scale *= Main.rand.NextFloat(0.85f, 1.2f);
        }

        switch (Mode)
        {
            default:
            case 0: // Constant velocity

                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature);

                if (Projectile.timeLeft < 20)
                {
                    Projectile.velocity *= 0.99f;
                    Curvature *= 0.99f;
                }

                break;

            case 1: // Slow down

                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature) * 0.98f;
                Curvature *= 0.99f;

                break;

            case 2: // Small

                break;
        }

        if (Main.rand.NextBool())
        {
            Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * Projectile.scale;
            Dust dust = Dust.NewDustPerfect(dustPos, DustID.Paint, Projectile.velocity * 0.5f, Alpha: 200, Scale: Main.rand.NextFloat(0.5f, 1.5f));
            dust.noGravity = true;
            dust.color = Color.CornflowerBlue with { A = 100 };
        }

        Projectile.frame = (int)(MiscTime / 3f) % 3;

        Lighting.AddLight(Projectile.Center, Color.SlateGray.ToVector3() * 0.5f);

        Time++;
        MiscTime++;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        Projectile.penetrate--;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.NPCDeath9 with { Pitch = Main.rand.NextFloat(), Volume = 0.1f, MaxInstances = 0 }, Projectile.Center);

        for (int i = 0; i < Main.rand.Next(8, 12); i++)
        {
            Vector2 offset = Main.rand.NextVector2Circular(1, 1);
            Vector2 velocity = Projectile.velocity * -0.1f + offset * Main.rand.NextFloat(2f, 5f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset * Projectile.width * Projectile.scale, DustID.Paint, velocity, Alpha: 200, Scale: Main.rand.NextFloat(1f, 2f));
            dust.noGravity = true;
            dust.color = Color.CornflowerBlue with { A = 100 };
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        bool small = Mode == 2;

        float scale = Utils.GetLerpValue(0, 8 * Projectile.scale, Time, true);

        Texture2D glow = Assets.Textures.GlowBig.Value;
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.White with { A = 0 } * 0.15f, Projectile.rotation, glow.Size() / 2, Projectile.scale * scale * 0.12f, 0, 0);

        Rectangle frame = texture.Frame(2, 3, small ? 1 : 0, Projectile.frame);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * 0.9f, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        float flareScale = Projectile.scale * scale * Utils.GetLerpValue(500, 250, Projectile.Distance(Main.LocalPlayer.Center), true);
        flareScale *= MathF.Sin(MiscTime / 2f) * 0.2f + 1f;
        Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition, flare.Frame(), Color.SlateGray with { A = 0 } * 0.3f, MathHelper.PiOver2, flare.Size() / 2, new Vector2(0.5f, flareScale * 1.33f), 0, 0);

        return false;
    }
}
