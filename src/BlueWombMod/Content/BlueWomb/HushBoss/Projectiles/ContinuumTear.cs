using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;

public sealed class ContinuumTear : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;

        Projectile.friendly = false;
        Projectile.hostile = true;

        Projectile.penetrate = 1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 600;
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float Curvature => ref Projectile.ai[1];

    public ref float MiscTime => ref Projectile.localAI[0];

    public override void OnSpawn(IEntitySource source)
    {
        MiscTime = Main.rand.Next(30);
        Projectile.scale *= Main.rand.NextFloat(0.8f, 1.2f);
    }

    private Vector2 originalVelocity;
    public ref Vector2 OriginalVelocity => ref originalVelocity;

    public override void AI()
    {
        if (Time == 0)
        {
            OriginalVelocity = Projectile.velocity;
        }

        if (Time < 30)
            Projectile.tileCollide = false;

        else if (Time == 30)
            Projectile.tileCollide = true;


        Vector3 cross = Vector3.Cross(new Vector3(OriginalVelocity, 0), Vector3.Forward * (Curvature > 0 ? 1 : -1));
        Vector2 perpendicular = new Vector2(cross.X, cross.Y);

        Projectile.Opacity = Utils.GetLerpValue(0, 40, Time, true) * Utils.GetLerpValue(10, 60, Projectile.timeLeft, true);
        Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * Projectile.Opacity);

        float beginCurve = Utils.GetLerpValue(0, 20, Time, true);
        float wave = MathF.Cos(Time / 18f);
        Projectile.velocity = OriginalVelocity + perpendicular * (wave * wave * wave - wave) * beginCurve * Curvature;
        OriginalVelocity = OriginalVelocity.SafeNormalize(Vector2.Zero) * (OriginalVelocity.Length() + 0.002f);

        if (Projectile.timeLeft < 60)
        {
            OriginalVelocity *= 0.97f;
        }

        MiscTime++;
        Time++;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.tileCollide = false;
        Projectile.velocity = oldVelocity;

        if (Projectile.timeLeft > 60)
        {
            Point pt = Projectile.Center.ToTileCoordinates();
            Point dir = originalVelocity.SafeNormalize(Vector2.Zero).ToPoint();

            for (int i = 0; i < 80; i++)
            {
                pt.X -= dir.X;
                pt.Y -= dir.Y;

                // Dust.QuickDust(pt, Color.Purple);
                if (i > 5 && WorldGen.SolidOrSlopedTile(pt.X, pt.Y))
                {
                    pt.X -= dir.X * 2;
                    pt.Y -= dir.Y * 2;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var tear = Projectile.NewProjectileDirect(Projectile.GetItemSource_FromThis(), pt.ToWorldCoordinates(), OriginalVelocity, Type, Projectile.damage, Projectile.knockBack);
                        tear.ai[1] = Projectile.ai[1];
                        tear.ai[2] = Projectile.ai[2];
                        tear.localAI[0] = Projectile.localAI[0];
                        tear.timeLeft = Projectile.timeLeft - 5;
                    }

                    break;
                }
            }
        }

        Projectile.damage = 0;
        Projectile.timeLeft = Math.Min(Projectile.timeLeft, 60);

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Time < 30 || Projectile.timeLeft < 50)
            return false;

        return base.Colliding(projHitbox, targetHitbox);
    }

    public override Color? GetAlpha(Color lightColor)
    {
        float progress = MiscTime / 25f % 1f;
        var purple = Color.Lerp(Color.Black, Color.Purple, Utils.GetLerpValue(0f, 0.33f, progress, true));
        var black = Color.Lerp(purple, Color.GhostWhite, Utils.GetLerpValue(0.33f, 0.66f, progress, true));
        var total = Color.Lerp(black, Color.Black, Utils.GetLerpValue(0.66f, 1f, progress, true));
        var fade = Color.Lerp(Color.Purple * Projectile.Opacity, total, Projectile.Opacity);
        return fade;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        lightColor = GetAlpha(lightColor) ?? lightColor;

        Texture2D texture = TextureAssets.Projectile[Type].Value;

        float scale = Utils.GetLerpValue(0, 8 * Projectile.scale, Time, true);

        Texture2D glow = Assets.Textures.GlowBig.Value;
        float glowScale = Projectile.scale * scale * 0.15f;
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.White with { A = 0 } * 0.2f * Projectile.Opacity, Projectile.rotation, glow.Size() / 2, glowScale, 0, 0);

        Rectangle frame = texture.Frame(1, 2, 0, 0);
        Rectangle glowFrame = texture.Frame(1, 2, 0, 1);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        Color glowColor = Color.Lerp(lightColor, Color.White, 0.5f) * 0.66f * Projectile.Opacity;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, glowFrame, glowColor with { A = 0 }, Projectile.rotation, glowFrame.Size() / 2, Projectile.scale * scale, 0, 0);

        float flareScale = Projectile.scale * scale * Utils.GetLerpValue(300, 150, Projectile.Distance(Main.LocalPlayer.Center), true);
        flareScale *= MathF.Sin(Time / 2f) * 0.2f + 1f;
        Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition, flare.Frame(), glowColor * 0.4f, MathHelper.PiOver2, flare.Size() / 2, new Vector2(0.5f, flareScale), 0, 0);

        return false;
    }
}
