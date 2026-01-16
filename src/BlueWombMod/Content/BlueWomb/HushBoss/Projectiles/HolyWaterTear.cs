using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;

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

    public ref float HostIndex => ref Projectile.ai[0];

    public ref float BehaviorMode => ref Projectile.ai[1];

    public ref float Curvature => ref Projectile.ai[2];

    public ref float MiscTime => ref Projectile.localAI[0];

    private int spawnTime;
    public ref int Time => ref spawnTime;

    public enum Behavior
    {
        Forward,
        SlowDown,
        Split,
        Small
    }

    public override void OnSpawn(IEntitySource source)
    {
        HostIndex = -1;
        MiscTime = Main.rand.Next(30);
        Projectile.scale *= Main.rand.NextFloat(0.85f, 1.2f);
    }

    public override void AI()
    {
        if (Time < 120 && HostIndex >= 0 && HostIndex < Main.npc.Length)
        {
            NPC npc = Main.npc[(int)HostIndex];
            if (!npc.active || npc.type != ModContent.NPCType<LittleHush>())
                HostIndex = -1;
            else
                Projectile.Center += npc.velocity * Utils.GetLerpValue(120, 30, Time, true);
        }

        switch (BehaviorMode)
        {
            default:
            case (int)Behavior.Forward:

                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature);

                if (Projectile.timeLeft < 20)
                {
                    Projectile.velocity *= 0.99f;
                    Curvature *= 0.99f;
                }

                break;

            case (int)Behavior.SlowDown:

                Projectile.velocity = Projectile.velocity.RotatedBy(Curvature) * 0.97f;
                Curvature *= 0.97f;

                break;

            case (int)Behavior.Split:

                Projectile.scale = 1f + MathF.Sin(MiscTime * 0.1f) * 0.2f;
                Projectile.velocity *= 0.984f;

                if (Projectile.timeLeft < 30)
                {
                    Projectile.velocity *= 0.95f;
                }

                break;

            case (int)Behavior.Small:

                Projectile.velocity *= 0.984f;

                break;
        }

        if (Main.rand.NextBool(4))
        {
            Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * Projectile.scale * 0.4f;
            Dust dust = Dust.NewDustPerfect(dustPos, DustID.AncientLight, Projectile.velocity * 0.5f, Alpha: 150, Scale: Main.rand.NextFloat());
            dust.noGravity = true;
            dust.color = Color.Blue with { A = 100 };
        }

        Projectile.frame = (int)(MiscTime / 3f) % 3;

        Lighting.AddLight(Projectile.Center, Color.SlateGray.ToVector3() * 0.5f);

        MiscTime++;
        Time++;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        Projectile.penetrate--;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.NPCDeath9 with { Pitch = Main.rand.NextFloat(), Volume = 0.1f, MaxInstances = 0 }, Projectile.Center);

        for (int i = 0; i < Main.rand.Next(4, 10); i++)
        {
            Vector2 offset = Main.rand.NextVector2Circular(1, 1);
            Vector2 velocity = Projectile.velocity * -0.1f + offset * Main.rand.NextFloat(2f, 5f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + offset * Projectile.width * Projectile.scale, DustID.AncientLight, velocity, Alpha: 150, Scale: Main.rand.NextFloat(1f, 2f));
            dust.noGravity = true;
            dust.color = Color.Blue with { A = 100 };
        }

        var particle = TearPopParticle.RequestNew(Projectile.Center, timeLeft: Main.rand.Next(5, 25), scale: Projectile.scale);
        ParticleEngine.Particles.Add(particle);

        if (Main.myPlayer == Projectile.owner && BehaviorMode == 2)
        {
            float randRot = Main.rand.NextFloat(-1f, 1f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = new Vector2(0, 4f).RotatedBy((float)i / 4 * MathHelper.TwoPi + randRot);
                Projectile tear = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<HolyWaterTear>(), Projectile.damage / 2, 0f);
                tear.ai[0] = HostIndex;
                tear.ai[1] = (int)Behavior.Small;
                tear.timeLeft = 70;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        bool small = BehaviorMode == 3;

        float scale = Utils.GetLerpValue(0, 8 * Projectile.scale, Time, true);

        Texture2D glow = Assets.Textures.GlowBig.Value;
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.White with { A = 0 } * 0.15f, Projectile.rotation, glow.Size() / 2, Projectile.scale * scale * 0.12f, 0, 0);

        Rectangle frame = texture.Frame(2, 3, small ? 1 : 0, Projectile.frame);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * 0.9f, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        float flareScale = Projectile.scale * scale * Utils.GetLerpValue(500, 250, Projectile.Distance(Main.LocalPlayer.Center), true);
        flareScale *= MathF.Sin(Time / 2f) * 0.2f + 1f;
        Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition, flare.Frame(), Color.SlateGray with { A = 0 } * 0.3f, MathHelper.PiOver2, flare.Size() / 2, new Vector2(0.5f, flareScale * 1.33f), 0, 0);

        return false;
    }
}