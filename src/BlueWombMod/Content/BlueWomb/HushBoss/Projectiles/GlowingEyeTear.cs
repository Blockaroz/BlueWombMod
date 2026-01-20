using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;

public sealed class GlowingEyeTear : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;

        Projectile.friendly = false;
        Projectile.hostile = true;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 1000;
    }

    public ref float BehaviorMode => ref Projectile.ai[0];

    public ref float TearColorType => ref Projectile.localAI[0];

    public ref float Curvature => ref Projectile.ai[1];

    public ref float MiscTime => ref Projectile.localAI[1];

    private int spawnTime;
    public ref int Time => ref spawnTime;

    public static Color GetColorFromType(int type)
    {
        return type switch
        {
            0 => new Color(106, 152, 198),
            1 => new Color(200, 173, 107),
            2 => new Color(156, 180, 111),
            3 => new Color(130, 127, 140),
            _ => Color.Black
        };
    }

    public enum Behavior
    {
        Forward,
        VelocityLoss,
        VelocityGain,
        BurstSpeed,
    }

    public override void AI()
    {
        if (MiscTime == 0)
        {
            MiscTime = Main.rand.Next(2);
            TearColor = GetColorFromType((int)TearColorType);
        }

        if (Projectile.timeLeft < 20)
        {
            Projectile.velocity *= 0.99f;
        }

        switch (BehaviorMode)
        {
            default:
            case (int)Behavior.Forward:

                Curvature *= 0.998f;
                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature);

                break;

            case (int)Behavior.VelocityLoss:

                if (Projectile.velocity.Length() > 2.5f)
                    Projectile.velocity *= 0.995f;

                Curvature *= 0.998f;
                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature);

                break;

            case (int)Behavior.VelocityGain:

                if (Projectile.velocity.Length() < 18f)
                    Projectile.velocity *= 1.0012f;
                else
                    Curvature *= 0.998f;

                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature);

                break;

            case (int)Behavior.BurstSpeed:

                if (Projectile.velocity.Length() < 3f)
                    Projectile.velocity *= 0.99f;
                else
                    Projectile.velocity *= 0.97f;

                if (Projectile.velocity.Length() < 0.7f)
                    Projectile.velocity *= 12f;

                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature * MathF.Sin(Time * 0.1f));

                break;
        }

        Lighting.AddLight(Projectile.Center, TearColor.ToVector3() * 0.5f);

        MiscTime++;
        Time++;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        Projectile.penetrate--;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.NPCDeath9 with { Pitch = Main.rand.NextFloat(), Volume = 0.05f, MaxInstances = 0 }, Projectile.Center);

        for (int i = 0; i < 15; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.AncientLight, Main.rand.NextVector2Circular(2, 2), newColor: TearColor * 0.5f, Scale: Main.rand.NextFloat(1f, 2f));
            dust.noGravity = true;
        }
    }

    public Color TearColor { get; set; }

    public override Color? GetAlpha(Color lightColor)
    {
        return (TearColor * (0.6f + 0.2f * MathF.Sin(MiscTime / 6f * MathHelper.Pi))) with { A = 200 };
    }

    public override bool PreDraw(ref Color lightColor)
    {
        lightColor = GetAlpha(lightColor) ?? lightColor;

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Texture2D glow = Assets.Textures.GlowBig.Value;

        float scale = Utils.GetLerpValue(0, 19 * Projectile.scale, Time, true);

        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), lightColor with { A = 0 } * 2f, Projectile.rotation, glow.Size() / 2, Projectile.scale * scale * 0.09f, 0, 0);

        Rectangle frame = texture.Frame(1, 2, 0, 0);
        Rectangle glowFrame = texture.Frame(1, 2, 0, 1);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        float flickerAmount = 0.6f + 0.4f * MathF.Sin(MiscTime / 6f * MathHelper.Pi);
        Color glowColor = Color.Lerp(lightColor * 0.5f, Color.White, flickerAmount) * Projectile.Opacity;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, glowFrame, glowColor with { A = 10 }, Projectile.rotation, glowFrame.Size() / 2, Projectile.scale * scale, 0, 0);

        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), lightColor with { A = 0 } * 0.67f, Projectile.rotation, glow.Size() / 2, Projectile.scale * scale * 0.15f, 0, 0);
        /*
        float flareScale = Projectile.scale * scale * Utils.GetLerpValue(250, 100, Projectile.Distance(Main.LocalPlayer.Center), true);
        flareScale *= MathF.Sin(Time) * 0.15f + 1f;
        Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition, flare.Frame(), lightColor with { A = 0 } * 0.3f, MathHelper.PiOver2, flare.Size() / 2, new Vector2(0.5f, flareScale * 1.5f), 0, 0);
        */
        return false;
    }
}