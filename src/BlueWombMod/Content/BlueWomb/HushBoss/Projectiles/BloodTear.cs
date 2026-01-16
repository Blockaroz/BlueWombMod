using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;

public sealed class BloodTear : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;

        Projectile.friendly = false;
        Projectile.hostile = true;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float BehaviorMode => ref Projectile.ai[1];

    public ref float Weight => ref Projectile.ai[2];

    public ref float MiscTime => ref Projectile.localAI[0];

    public enum Behavior
    {
        Forward,
        Fall,
        BigSplit
    }

    public override void OnSpawn(IEntitySource source)
    {
        MiscTime = Main.rand.Next(30);
        Projectile.scale *= Main.rand.NextFloat(0.7f, 1.2f) + Weight * 0.1f;
    }

    public override void AI()
    {
        switch (BehaviorMode)
        {
            default:
            case (int)Behavior.Forward:

                Projectile.velocity *= 1f - Weight * 0.1f;

                break;

            case (int)Behavior.Fall:

                Projectile.velocity.Y += 0.2f * Weight;

                break;

            case (int)Behavior.BigSplit:

                if (Time == 0)
                {
                    Projectile.scale = Projectile.scale * 0.5f + 0.5f;
                    Projectile.Resize(24, 24);
                }

                if (Time > 30)
                    Projectile.velocity *= 0.93f;

                if (Time > 90)
                    Projectile.Kill();

                break;
        }

        Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * Projectile.scale;
        Dust bleed = Dust.NewDustPerfect(dustPos, DustID.Blood, Projectile.velocity * 0.5f, Scale: Main.rand.NextFloat(0.5f, 1.5f));
        bleed.noGravity = true;

        Projectile.frame = (int)(MiscTime / 5) % 2;

        Projectile.rotation = Projectile.velocity.X * 0.015f * Projectile.scale;

        Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.5f);

        Time++;
        MiscTime++;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 0.5f, MaxInstances = 0 }, Projectile.Center);

        for (int i = 0; i < Main.rand.Next(8, 12); i++)
        {
            Vector2 offset = Main.rand.NextVector2Circular(1, 1);
            Vector2 velocity = Projectile.velocity * -0.1f + offset * Main.rand.NextFloat(2f, 5f);
            Dust bleed = Dust.NewDustPerfect(Projectile.Center + offset * Projectile.width * Projectile.scale, DustID.Blood, velocity, Scale: Main.rand.NextFloat(1f, 2f));
            bleed.noGravity = true;
        }

        if (Main.myPlayer == Projectile.owner && BehaviorMode == 2)
        {
            float randRot = Main.rand.NextFloat(-1f, 1f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = new Vector2(0, 4f).RotatedBy((float)i / 4 * MathHelper.TwoPi + randRot);
                Projectile tear = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BloodTear>(), Projectile.damage / 2, 0f);
                tear.scale *= 0.8f;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        float scale = Utils.GetLerpValue(0, 8 * Projectile.scale, Time, true);
        bool large = BehaviorMode == 2;

        Texture2D glow = Assets.Textures.GlowBig.Value;
        float glowScale = Projectile.scale * scale * 0.25f * (large ? 1f : 0.6f);
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.White with { A = 0 } * 0.07f, Projectile.rotation, glow.Size() / 2, glowScale, 0, 0);

        Rectangle frame = texture.Frame(2, 2, large ? 1 : 0, Projectile.frame);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * 1.1f, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        return false;
    }
}