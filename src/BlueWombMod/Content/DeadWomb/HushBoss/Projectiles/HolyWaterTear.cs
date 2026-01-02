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
            Projectile.scale *= Main.rand.NextFloat(0.8f, 1.2f);
        }

        switch (Mode)
        {
            default:
            case 0: // Fly forward

                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature);

                if (Projectile.timeLeft < 20)
                {
                    Curvature *= 0.98f;
                    Projectile.velocity *= 0.98f;
                }

                break;

            case 1: // Split on death

                break;
        }

        Projectile.frame = (int)(MiscTime / 3f) % 2;

        Lighting.AddLight(Projectile.Center, Color.SlateGray.ToVector3() * 0.5f);

        Time++;
        MiscTime++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        bool small = Projectile.scale <= 0.8f;
        if (small)
        {
            Projectile.scale += 0.2f;
        }

        float scale = Utils.GetLerpValue(0, 8 * Projectile.scale, Time, true);

        Texture2D glow = Assets.Textures.GlowBig.Value;
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.White with { A = 0 } * 0.2f, Projectile.rotation, glow.Size() / 2, Projectile.scale * scale * 0.15f, 0, 0);

        Rectangle frame = texture.Frame(2, 2, small ? 1 : 0, Projectile.frame);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * 1.1f, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        float flareScale = Projectile.scale * scale * Utils.GetLerpValue(300, 150, Projectile.Distance(Main.LocalPlayer.Center), true);
        flareScale *= MathF.Sin(Main.GlobalTimeWrappedHourly * 40) * 0.2f + 1f;
        Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition, flare.Frame(), Color.SlateGray with { A = 0 } * 0.5f, MathHelper.PiOver2, flare.Size() / 2, new Vector2(0.5f, flareScale * 1.33f), 0, 0);

        return false;
    }
}
