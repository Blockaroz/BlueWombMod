using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.Particles;
using log4net.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss.Minions.Flies;

public sealed class AttackFly : ModNPC
{
    public override void SetStaticDefaults()
    {
    }

    public override void SetDefaults()
    {
        NPC.width = 28;
        NPC.height = 28;

        NPC.lifeMax = 50;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.9f;

        NPC.damage = 30;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath35 with { Pitch = 0.5f };
    }

    public const int STATE_ATTACK = 0;
    public const int STATE_PASSIVE = 1;

    public ref float State => ref NPC.ai[0];
    public ref float Time => ref NPC.ai[1];
    public ref float SpawnTime => ref NPC.ai[2];

    public bool Passive { get => State == STATE_PASSIVE; set => State = value ? STATE_PASSIVE : STATE_ATTACK;  }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.scale *= Main.rand.NextFloat(0.75f, 1.1f);
        NPC.lifeMax = (int)(NPC.lifeMax * NPC.scale);
        NPC.life = NPC.lifeMax;
    }

    public override bool? CanFallThroughPlatforms()
    {
        return true;
    }

    public override void AI()
    {
        if (NPC.HasBuff(BuffID.Frozen))
        {
            return;
        }

        if (NPC.HasBuff(BuffID.Confused))
        {
            Passive = true;
        }

        if (SpawnTime < 20)
        {
            SpawnTime++;
            NPC.velocity *= 0.9f;
        }
        else
        {
            if (Passive)
            {
                Time--;

                NPC.velocity *= 0.9f;

                if (Time % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity += Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.2f);
                    NPC.netUpdate = true;
                }

                if (Time <= 0)
                {
                    Passive = false;
                    Time = 0;
                }
            }
            else
            {
                NPC.velocity *= 0.97f;

                NPC.TargetClosest();

                NPCAimedTarget target = NPC.GetTargetData();

                if (!target.Invalid)
                {
                    if (Time % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.velocity += Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat();
                        NPC.netUpdate = true;
                    }

                    NPC.velocity += NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 0.2f;
                }

                if (NPC.HasBuff(BuffID.Slow))
                {
                    NPC.velocity *= 0.68f;
                }
            }
        }

        NPC.rotation = NPC.velocity.X * 0.015f * NPC.scale;

        if (Main.rand.NextBool(Passive ? 50 : 100))
        {
            Vector2 flyVel = NPC.velocity * 0.5f + Main.rand.NextVector2Circular(2, 2);
            var flyParticle = LittleAngryBugParticle.RequestNew(NPC.Center + Main.rand.NextVector2Circular(40, 40), flyVel, Main.rand.Next(100, 200), 1f);
            ParticleEngine.Particles.Add(flyParticle);
        }
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
        Passive = true;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = NPC.Hitbox;
    }

    public ref float AnimationFrame => ref NPC.localAI[0];

    private int angerFrameCounter;
    public ref float AngerFrame => ref NPC.localAI[1];

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        
        if (NPC.frameCounter > (Passive ? 3 : 2))
        {
            NPC.frameCounter = 0;

            AnimationFrame++;
            if (AnimationFrame >= 4)
            {
                AnimationFrame = 0;
            }
        }

        angerFrameCounter++;
        if (angerFrameCounter > 2)
        {
            angerFrameCounter = 0;
            if (Passive && !NPC.IsABestiaryIconDummy)
            {
                AngerFrame = 0;
            }
            else
            {
                AngerFrame = AngerFrame > 0 ? 0 : 1;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;

        Rectangle frame = texture.Frame(2, 4, (int)AngerFrame, (int)AnimationFrame);

        float scale = NPC.scale * Utils.GetLerpValue(-5, 15, SpawnTime, true);
        if (NPC.IsABestiaryIconDummy)
        {
            scale = 1f;
        }

        spriteBatch.Draw(texture, NPC.Center - screenPos, frame, drawColor * 1.2f, NPC.rotation, frame.Size() / 2f, scale, 0, 0);

        return false;
    }
}