using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.BlueWomb;
using BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Minions;

public sealed class BlueBoil : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 32;
        NPC.height = 32;

        NPC.lifeMax = 100;
        NPC.defense = 15;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.5f;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath11;
        NPC.behindTiles = true;

        SpawnModBiomes = [ModContent.GetInstance<BlueWombBiome>().Type];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(new FlavorTextBestiaryInfoElement(Mod.GetLocalization($"NPCs.{nameof(BlueBoil)}.FlavorText").Key));
    }

    public ref float ShootTime => ref NPC.ai[0];
    public ref float Time => ref NPC.ai[1];
    public ref float SpawnTime => ref NPC.ai[2];

    public override void OnSpawn(IEntitySource source)
    {
        NPC.scale *= Main.rand.NextFloat(0.9f, 1.25f);
        NPC.lifeMax = (int)(NPC.lifeMax * NPC.scale);
        NPC.life = NPC.lifeMax;
    }

    public override void AI()
    {
        DrawScale = Vector2.One;

        if (NPC.HasBuff(BuffID.Frozen))
        {
            NPC.velocity *= 0.2f;
            return;
        }

        float aimOffset = 0f;
        if (NPC.HasBuff(BuffID.Confused))
        {
            aimOffset = 2f;
        }

        if (SpawnTime < 20)
        {
            SpawnTime++;
            NPC.velocity *= 0.9f;
        }
        else
        {
            NPC.velocity *= 0.7f;

            if (Time % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.velocity += Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat();
                NPC.netUpdate = true;
            }

            NPC.TargetClosest();

            NPCAimedTarget target = NPC.GetTargetData();

            const int WaitTime = 135;
            const int SpitTime = 33;

            if (ShootTime > WaitTime || (!target.Invalid && NPC.Distance(target.Center) < 800))
            {
                if (ShootTime == (WaitTime + SpitTime / 2))
                {
                    SoundEngine.PlaySound(SoundID.Item111 with { MaxInstances = 0, Pitch = 0.5f, PitchVariance = 0.1f }, NPC.Center);

                    if (!target.Invalid && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 shotVel = NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.05f) * Main.rand.NextFloat(4f, 6f);
                        Projectile blood = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, shotVel, ModContent.ProjectileType<BloodTear>(), 40, 0f);
                        blood.ai[0] = 0;
                        blood.scale *= 0.9f;
                    }
                }

                Vector2 squashIn = Vector2.Lerp(Vector2.One, new Vector2(1.5f, 0.5f), Utils.GetLerpValue(WaitTime + SpitTime / 5f, WaitTime + SpitTime / 3f, ShootTime, true));
                Vector2 squashOut = Vector2.Lerp(Vector2.One, new Vector2(0.6f, 1.5f), MathF.Sqrt(Utils.GetLerpValue(SpitTime / 1.1f, SpitTime / 2f, ShootTime - WaitTime, true)));
                DrawScale = Vector2.Lerp(squashIn, squashOut, Utils.GetLerpValue(SpitTime / 2.5f, SpitTime / 2f, ShootTime - WaitTime, true));

                ShootTime++;

                if (ShootTime >= WaitTime + SpitTime)
                {
                    ShootTime = 0;
                }
            }
        }

        NPC.FaceTarget();
    }

    public override void OnKill()
    {
        var flyParticle = LittleAngryBugParticle.RequestNew(NPC.Center, Vector2.Zero, Main.rand.Next(30, 60), Main.rand.NextFloat(0.5f, 1.5f));
        ParticleEngine.Particles.Add(flyParticle);

        for (int i = 0; i < Main.rand.Next(5, 10); i++)
        {
            Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.Blood, Main.rand.NextVector2Circular(5, 5), Scale: 1.5f);
            dust.noGravity = true;
        }

        for (int i = 0; i < Main.rand.Next(8, 15); i++)
        {
            Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.FireflyHit, Main.rand.NextVector2Circular(5, 5), Scale: 2f * NPC.scale);
            dust.noGravity = true;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        Time = Main.rand.Next(90, 150);
    }

    public override bool? CanFallThroughPlatforms()
    {
        return true;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = NPC.Hitbox;
    }

    public ref float AnimationFrame => ref NPC.localAI[0];

    public override void FindFrame(int frameHeight)
    {
        if (NPC.HasBuff(BuffID.Frozen))
        {
            return;
        }

        NPC.frameCounter++;

        if (NPC.frameCounter > 4)
        {
            NPC.frameCounter = 0;

            AnimationFrame++;
            if (AnimationFrame >= 4)
            {
                AnimationFrame = 0;
            }
        }
    }

    private Vector2 drawScale;
    public ref Vector2 DrawScale => ref drawScale;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;

        Rectangle frame = texture.Frame(1, 4, 0, (int)AnimationFrame);

        float scale = NPC.scale * Utils.GetLerpValue(-5, 15, SpawnTime, true);
        if (NPC.IsABestiaryIconDummy)
        {
            DrawScale = Vector2.One;
            scale = 1f;
        }

        SpriteEffects flip = NPC.direction < 0 ? SpriteEffects.FlipHorizontally : 0;
        spriteBatch.Draw(texture, NPC.Center - screenPos, frame, drawColor * 1.2f, NPC.rotation, frame.Size() / 2f, DrawScale * scale, flip, 0);

        return false;
    }
}