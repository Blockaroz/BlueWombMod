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
using static tModPorter.ProgressUpdate;

namespace BlueWombMod.Content.DeadWomb.HushBoss.Projectiles;

public sealed class HolySkyCrack : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;

        Projectile.timeLeft = 120;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.hide = true;

        Projectile.damage = 20;
    }

    public ref float Radius => ref Projectile.ai[0];

    public bool Bursting { get => Projectile.ai[1] == 1; set => Projectile.ai[1] = value ? 1 : 0; }

    public ref float Time => ref Projectile.ai[2];

    public override void AI()
    {
        if (Bursting)
        {
            return;
        }

        Radius = 50f;

        if (!Bursting && Projectile.timeLeft == 1)
        {
            Bursting = true;
            Projectile.timeLeft = 30;

            SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Volume = 2f, Pitch = 1f }, Projectile.Center);

            int count = 33;
            for (int i = 0; i < (int)count; i++)
            {
                Vector2 offset = new Vector2(0, 1f).RotatedByRandom(MathHelper.TwoPi);
                Dust light = Dust.NewDustPerfect(Projectile.Center + offset * Main.rand.NextFloat(Radius), DustID.FireworksRGB, Velocity: offset * Main.rand.NextFloat(8f), newColor: Color.GhostWhite * 0.5f, Scale: Main.rand.NextFloat(0.5f, 1.5f));
                light.noGravity = true;
            }
        }

        float progress = Utils.GetLerpValue(60, 0, Projectile.timeLeft, true);

        if (Projectile.timeLeft % 5 == 0)
        {
            int count = (int)(20 * progress) + 2;
            for (int i = 0; i < (int)count; i++)
            {
                Vector2 offset = new Vector2(0, Radius).RotatedByRandom(MathHelper.TwoPi);
                Vector2 velocity = -offset.RotatedBy(MathHelper.PiOver2 * Main.rand.NextBool().ToDirectionInt()) * Main.rand.NextFloat(0.05f);
                Dust light = Dust.NewDustPerfect(Projectile.Center + offset * Main.rand.NextFloat(1f, 1.1f), DustID.AncientLight, Velocity: velocity, newColor: Color.GhostWhite * 0.1f, Scale: Main.rand.NextFloat());
                light.noGravity = true;
                light.fadeIn = 0.5f;
            }
        }

        Lighting.AddLight(Projectile.Center, Color.GhostWhite.ToVector3() * 0.5f * progress);

        Time++;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Bursting)
        {
            var direction = Projectile.DirectionTo(targetHitbox.Center()).SafeNormalize(Vector2.Zero);
            var distance = Math.Min(Radius - 5, Projectile.Distance(targetHitbox.Center()));
            return targetHitbox.Contains((Projectile.Center + direction * distance).ToPoint());
        }

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!Bursting)
        {
            Texture2D glow = TextureAssets.Projectile[Type].Value;
            float fade = Utils.GetLerpValue(0, 20, Time, true);
            float scale = Utils.GetLerpValue(-5, 5, Projectile.timeLeft, true) * Radius / 31f * (1f + MathF.Sin(Projectile.timeLeft) * 0.05f);
            var color = Color.White with { A = 0 } * (0.1f + Utils.GetLerpValue(10, 0, Projectile.timeLeft, true)) * fade * (1f + MathF.Sin(Projectile.timeLeft) * 0.15f);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), color, 0f, glow.Size() / 2, scale, 0, 0);
        }

        return false;
    }
}
