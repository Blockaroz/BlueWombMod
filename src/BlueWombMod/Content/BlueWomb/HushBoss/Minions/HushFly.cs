using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Minions;

public sealed class HushFly : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 22;
        NPC.height = 22;

        NPC.lifeMax = 80;
        NPC.defense = 10;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.1f;
        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1 with { Pitch = 0.33f };

        NPC.damage = 40;

        SpawnModBiomes = [ModContent.GetInstance<BlueWombBiome>().Type];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(new FlavorTextBestiaryInfoElement(Mod.GetLocalization($"NPCs.{nameof(HushFly)}.FlavorText").Key));
    }

    public enum HushFlyShape
    {
        Orbit, 
        Circle, 
        Triangle, 
        Square,
        Pentagon,
        Star
    }

    public ref float LeaderIndex => ref NPC.ai[0];
    public ref float Position => ref NPC.ai[1];
    public ref float Time => ref NPC.ai[2];

    public ref float WheelTime => ref NPC.localAI[1];

    public override void OnSpawn(IEntitySource source)
    {
        LeaderIndex = -1;

        NPC.scale *= Main.rand.NextFloat(0.8f, 1.15f);
        NPC.netUpdate = true;
    }

    public override void AI()
    {
        if (NPC.HasBuff(BuffID.Frozen))
        {
            NPC.velocity *= 0.2f;
            return;
        }

        if (LeaderIndex >= 0 && LeaderIndex < Main.npc.Length)
        {
            NPC.dontTakeDamage = false;

            NPC leader = Main.npc[(int)(LeaderIndex)];
            if (!leader.active)
                LeaderIndex = -1;
            else
                DoShapeAction(leader);
        }
        else
        {
            if (Position > -1)
            {
                Time = 0;
                Position = -1;
            }

            NPC.velocity *= 0.97f;

            if (Time % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.velocity += Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.2f);
                NPC.netUpdate = true;
            }

            NPC.dontTakeDamage = true;
            NPC.damage = 0;

            NPC.Opacity = Utils.GetLerpValue(60, 20, Time, true);

            if (Time > 60)
                NPC.active = false;
        }

        if (Main.rand.NextBool(50))
        {
            Vector2 flyVel = NPC.velocity * 0.5f + Main.rand.NextVector2Circular(2, 2);
            var flyParticle = LittleAngryBugParticle.RequestNew(NPC.Center + Main.rand.NextVector2Circular(40, 40), flyVel, Main.rand.Next(100, 200), 1f);
            ParticleEngine.Particles.Add(flyParticle);
        }
        
        Time++;

        NPC.direction = NPC.velocity.X < 0 ? -1 : 1;
        NPC.rotation = NPC.velocity.X * 0.015f * NPC.scale;
    }

    private void DoShapeAction(NPC leader)
    {
        if (leader is null)
            return;
        if (leader.type == ModContent.NPCType<Hush>())
        {
            NPC.active = false;
        }
        else if (leader.type == ModContent.NPCType<HushFlyLeader>())
        {
            var shape = leader.ai[0];
            float spinTime = leader.ai[1];
            float siblingCount = leader.ai[2];
            float spawnTime = Utils.GetLerpValue(0, 80, Time, true);

            Vector2 targetPosition = leader.Center;
            if (siblingCount > 0)
            {
                switch (shape)
                {
                    default:
                    case (int)HushFlyShape.Orbit:
                    case (int)HushFlyShape.Circle:

                        targetPosition += new Vector2(0f, 40f + siblingCount * 2f).RotatedBy(spinTime * 0.07f - Position / siblingCount * MathHelper.TwoPi);

                        break;
                    case (int)HushFlyShape.Triangle:

                        float angle = spinTime * 0.025f + Position / siblingCount * MathHelper.TwoPi;
                        targetPosition += new Vector2(0f, GetPolygonRadius(sides: 3, angle) * (60f + siblingCount * 2)).RotatedBy(angle + spinTime * 0.004f);

                        break;
                    case (int)HushFlyShape.Square:

                        angle = -spinTime * 0.04f + Position / siblingCount * MathHelper.TwoPi;
                        targetPosition += new Vector2(0f, GetPolygonRadius(sides: 4, angle) * (50f + siblingCount * 2)).RotatedBy(angle + spinTime * 0.003f);

                        break;
                    case (int)HushFlyShape.Pentagon:

                        angle = -spinTime * 0.04f + Position / siblingCount * MathHelper.TwoPi;
                        targetPosition += new Vector2(0f, GetPolygonRadius(sides: 5, angle) * (50f + siblingCount * 2)).RotatedBy(angle - spinTime * 0.001f);

                        break;
                    case (int)HushFlyShape.Star:

                        angle = spinTime * 0.06f - Position / siblingCount * MathHelper.TwoPi;
                        targetPosition += new Vector2(0f, GetStarRadius(points: 5, angle, squashIn: 3.15f) * (70f + siblingCount * 2)).RotatedBy(angle - spinTime * 0.003f);

                        break;
                }
            }

            NPC.velocity *= 0.8f;
            NPC.velocity += (targetPosition - NPC.Center) * 0.05f * spawnTime;

        }
        else
        {
            LeaderIndex = -1;
            Time = 0;
        }
    }

    // from https://www.desmos.com/calculator/d9zrtkf7rx

    private float GetPolygonRadius(float sides, float theta)
    {
        float numerator = MathF.Cos(MathHelper.Pi / sides);
        float denominator = MathF.Cos(theta - (MathHelper.TwoPi / sides) * MathF.Floor((sides * theta + MathHelper.Pi) / MathHelper.TwoPi));
        return numerator / denominator; 
    }

    private float GetStarRadius(float points, float theta, float squashIn = 2.5f)
    {
        const float sharpness = 1f;
        float numerator = MathF.Cos((2f * MathF.Asin(sharpness) + MathHelper.Pi * squashIn) / (2 * points));
        float denominator = MathF.Cos((2f * MathF.Asin(sharpness * MathF.Cos(points * theta)) + MathHelper.Pi * squashIn) / (2 * points));
        return numerator / denominator;
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

        if (NPC.frameCounter > 3)
        {
            NPC.frameCounter = 0;

            AnimationFrame++;
            if (AnimationFrame >= 4)
            {
                AnimationFrame = 0;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;

        Rectangle frame = texture.Frame(1, 4, 0, (int)AnimationFrame);

        float scale = NPC.scale * Utils.GetLerpValue(-25, -5, Time, true);
        if (NPC.IsABestiaryIconDummy)
        {
            scale = 1f;
        }

        spriteBatch.Draw(texture, NPC.Center - screenPos, frame, drawColor * 1.2f * NPC.Opacity, NPC.rotation, frame.Size() / 2f, scale, 0, 0);
        // Utils.DrawBorderString(spriteBatch, Position.ToString(), NPC.Bottom - screenPos, Color.White, anchorx: 0.5f, anchory: 0.5f);

        return false;
    }
}

public sealed class HushFlyLeader : ModNPC
{
    public override string Texture => ModContent.GetInstance<HushFly>().Texture;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.CantTakeLunchMoney[Type] = true;
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.width = 80;
        NPC.height = 80;

        NPC.lifeMax = 150;
        NPC.noGravity = true;
        NPC.knockBackResist = 0f;
        NPC.noTileCollide = true;

        NPC.dontTakeDamage = true;
        NPC.dontCountMe = true;
        NPC.Opacity = 0f;
        NPC.timeLeft = 60;
        //NPC.hide = true;
    }

    public ref float Shape => ref NPC.ai[0];

    public ref float SpinTime => ref NPC.ai[1];

    public ref float Children => ref NPC.ai[2];

    public ref float Time => ref NPC.ai[3];

    public override void AI()
    {
        SpinTime += NPC.velocity.X < 0 ? -1 : 1;

        int children = 0;
        int position = 0;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.type == ModContent.NPCType<HushFly>() && npc.ai[0] == NPC.whoAmI)
            {
                children++;
                npc.ai[1] = position++;
            }
        }

        Children = children;

        if (Time < 600 || Children <= 0)
        {
            if (Children > 0)
            {
                NPC.timeLeft = 60;

                if (Time % 15 == 0)
                {
                    NPC.TargetClosest();
                    var target = NPC.GetTargetData();
                    if (!target.Invalid)
                    {
                        NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(target.Center), 0.67f).SafeNormalize(Vector2.Zero) * 3f;
                    }
                }
            }
            else
            {
                NPC.timeLeft--;

                if (NPC.timeLeft <= 0)
                    NPC.active = false;
            }
        }
        else
        {
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.active = false;
        }


        Time++;
    }

    public override bool CheckActive()
    {
        return false;
    }

    public override bool CheckDead()
    {
        return false;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = Rectangle.Empty;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // Utils.DrawBorderString(spriteBatch, "+", NPC.Center - screenPos, Color.White, anchorx: 0.5f, anchory: 0.5f);
        return false;
    }
}