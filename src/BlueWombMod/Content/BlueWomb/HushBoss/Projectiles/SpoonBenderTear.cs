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

public sealed class SpoonBenderTear : ModProjectile
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

    public ref float HostIndex => ref Projectile.ai[0];

    public ref float Time => ref Projectile.ai[1];

    public ref float MiscTime => ref Projectile.localAI[0];

    public override void AI()
    {
        if (HostIndex > 0 && HostIndex <= Main.npc.Length)
        {
            NPC npc = Main.npc[(int)HostIndex - 1];
            if (npc.active && npc.life > 5 && !npc.friendly)
            {
                NPCAimedTarget target = npc.GetTargetData();
                if (!target.Invalid)
                {
                    Projectile.velocity += Projectile.DirectionTo(target.Center) * 0.25f * Utils.GetLerpValue(0, 30, Time, true) * Utils.GetLerpValue(0, 100, Projectile.timeLeft, true);
                }
            }
        }

        Projectile.velocity *= 0.99f;

        Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) * Projectile.scale;
        Dust dust = Dust.NewDustPerfect(dustPos, DustID.Shadowflame, Projectile.velocity * 0.5f, Scale: Main.rand.NextFloat(0.33f, 1f));
        dust.noGravity = true;

        Projectile.rotation = Projectile.velocity.X * 0.015f * Projectile.scale;

        Projectile.frame = (int)(MiscTime / 3f) % 3;

        Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.5f);

        Time++;
        MiscTime++;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 0.5f, MaxInstances = 0 }, Projectile.Center);

        var particle = HomingTearPopParticle.RequestNew(Projectile.Center, timeLeft: Main.rand.Next(15, 25), scale: Projectile.scale);
        ParticleEngine.Particles.Add(particle);

        for (int i = 0; i < Main.rand.Next(8, 12); i++)
        {
            Vector2 offset = Main.rand.NextVector2Circular(1, 1);
            Vector2 velocity = Projectile.velocity * -0.1f + offset * Main.rand.NextFloat(2f, 5f);
            Dust bleed = Dust.NewDustPerfect(Projectile.Center + offset * Projectile.width * Projectile.scale, DustID.Shadowflame, velocity, Scale: Main.rand.NextFloat(0.5f, 1f));
            bleed.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        float scale = Utils.GetLerpValue(0, 8 * Projectile.scale, Time, true);

        Texture2D glow = Assets.Textures.GlowBig.Value;
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.White with { A = 0 } * 0.15f, Projectile.rotation, glow.Size() / 2, Projectile.scale * scale * 0.12f, 0, 0);

        Rectangle frame = texture.Frame(1, 3, 0, Projectile.frame);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * 1.2f, Projectile.rotation, frame.Size() / 2, Projectile.scale * scale, 0, 0);

        float flareScale = Projectile.scale * scale * Utils.GetLerpValue(500, 250, Projectile.Distance(Main.LocalPlayer.Center), true);
        flareScale *= MathF.Sin(Time / 2f) * 0.2f + 1f;
        Texture2D flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition, flare.Frame(), Color.Indigo with { A = 0 } * 0.4f, MathHelper.PiOver2, flare.Size() / 2, new Vector2(0.5f, flareScale), 0, 0);

        return false;
    }
}