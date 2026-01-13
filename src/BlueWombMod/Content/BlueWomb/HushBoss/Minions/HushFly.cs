using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1 with { Pitch = 0.33f };

        NPC.damage = 30;

        SpawnModBiomes = [ModContent.GetInstance<BlueWombBiome>().Type];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(new FlavorTextBestiaryInfoElement(Mod.GetLocalizationKey($"Bestiary.{NPC.TypeName}")));
    }

    public ref float State => ref NPC.ai[0];
    public ref float Time => ref NPC.ai[1];
    public ref float SpawnTime => ref NPC.ai[2];

    public enum HushFlyShape
    {
        Orbit, Circle, Triangle, Square, Pentagon, Star
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.scale *= Main.rand.NextFloat(0.9f, 1.1f);
        NPC.lifeMax = (int)(NPC.lifeMax * NPC.scale);
        NPC.life = NPC.lifeMax;
    }

    public override void AI()
    {
        if (NPC.HasBuff(BuffID.Frozen))
        {
            NPC.velocity *= 0.2f;
            return;
        }

        NPC.direction = NPC.velocity.X < 0 ? -1 : 1;
        NPC.rotation = NPC.velocity.X * 0.015f * NPC.scale;

        if (SpawnTime < 20)
        {
            SpawnTime++;
        }
        else
        {

        }

        if (Main.rand.NextBool(50))
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

        float scale = NPC.scale * Utils.GetLerpValue(-5, 15, SpawnTime, true);
        if (NPC.IsABestiaryIconDummy)
        {
            scale = 1f;
        }

        spriteBatch.Draw(texture, NPC.Center - screenPos, frame, drawColor * 1.2f, NPC.rotation, frame.Size() / 2f, scale, 0, 0);

        return false;
    }
}