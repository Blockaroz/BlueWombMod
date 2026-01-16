using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
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
        NPC.width = 28;
        NPC.height = 28;

        NPC.lifeMax = 150;
        NPC.defense = 10;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.1f;
        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1 with { Pitch = 0.33f };

        NPC.damage = 30;

        SpawnModBiomes = [ModContent.GetInstance<BlueWombBiome>().Type];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(new FlavorTextBestiaryInfoElement(Mod.GetLocalization($"NPCs.{NPC.TypeName}.FlavorText").Key));
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
        NPC.scale *= Main.rand.NextFloat(0.9f, 1.1f);
        NPC.lifeMax = (int)(NPC.lifeMax * NPC.scale);
        NPC.life = NPC.lifeMax;

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

            NPC.velocity *= 0.98f;

            if (Time % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.velocity += Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.2f);
                NPC.netUpdate = true;
            }

            NPC.dontTakeDamage = true;
            NPC.Opacity = Utils.GetLerpValue(100, 20, Time, true);

            if (Time > 100)
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

            Vector2 targetPosition = leader.Center;
            switch (shape)
            {
                default:
                case (int)HushFlyShape.Orbit:
                case (int)HushFlyShape.Circle:

                    if (siblingCount > 0)
                        targetPosition += new Vector2(0, 30 + siblingCount * 2).RotatedBy(spinTime * 0.01f + Position / siblingCount * MathHelper.TwoPi);
                    NPC.velocity = (targetPosition - NPC.Center) * 0.3f;

                    break;
            }
        }
        else
        {
            LeaderIndex = -1;
            Time = 0;
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

        if (Children > 0 && Time < 600)
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
        Utils.DrawBorderString(spriteBatch, "+", NPC.Center - screenPos, Color.White, anchorx: 0.5f, anchory: 0.5f);
        return false;
    }
}