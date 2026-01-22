using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.BlueWomb;
using BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.Actions.NPCs;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Minions;

public sealed class BlueBoil : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 44;
        NPC.height = 44;

        NPC.lifeMax = 150;
        NPC.defense = 15;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0.5f;

        NPC.HitSound = SoundID.NPCHit13;
        NPC.DeathSound = SoundID.NPCDeath21;

        SpawnModBiomes = [ModContent.GetInstance<BlueWombBiome>().Type];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(new FlavorTextBestiaryInfoElement(Mod.GetLocalization($"NPCs.{nameof(BlueBoil)}.FlavorText").Key));
    }

    public ref float ShootTime => ref NPC.ai[0];
    public ref float Time => ref NPC.ai[1];
    public ref float SpawnTime => ref NPC.ai[2];

    public ref float Variant => ref NPC.localAI[0];

    public override void OnSpawn(IEntitySource source)
    {
        NPC.scale *= Main.rand.NextFloat(0.9f, 1.25f);
        NPC.lifeMax = (int)(NPC.lifeMax * NPC.scale);
        NPC.life = NPC.lifeMax;
    }

    private int CountAir(int x, int y, out Vector2 direction)
    {
        int count = 0;
        direction = Vector2.Zero;
        if (WorldGen.SolidTile(x, y - 1))
            count++;
        else
            direction.Y++;

        if (WorldGen.SolidTile(x, y + 1))
            count++;
        else
            direction.Y--;

        if (WorldGen.SolidTile(x - 1, y))
            count++;
        else
            direction.X++;

        if (WorldGen.SolidTile(x + 1, y))
            count++;
        else
            direction.X--;


        return count;
    }

    public override void AI()
    {
        if (NPC.HasBuff(BuffID.Frozen))
            return;

        DrawScale = Vector2.One;

        if (SpawnTime < 20)
        {
            NPC.dontTakeDamage = true;

            if (SpawnTime == 0)
            {
                Vector2 newPosition = Vector2.Zero;
                Vector2 newAngle = Vector2.Zero;
                int surfaces = 0;

                for (int j = -10; j < 10; j++)
                {
                    for (int i = -10; i < 10; i++)
                    {
                        Point tilePos = NPC.Center.ToTileCoordinates() + new Point(i, j);

                        if (WorldGen.SolidTile(tilePos.X, tilePos.Y) && CountAir(tilePos.X, tilePos.Y, out Vector2 airDirection) < 4)
                        {
                            var addPos = tilePos.ToWorldCoordinates();

                            if (surfaces == 0)
                            {
                                newPosition = addPos;
                                newAngle = airDirection;
                                surfaces++;
                            }
                            else
                            {
                                var addRot = airDirection;

                                newPosition += addPos;
                                newAngle += addRot;
                                surfaces++;
                            }
                        }
                    }
                }

                if (surfaces > 0)
                {
                    float angle = MathHelper.WrapAngle((newAngle / surfaces).ToRotation());
                    Vector2 center = newPosition / surfaces;

                    NPC.rotation = angle - MathHelper.PiOver2;
                    NPC.Center = center - (newAngle / surfaces) * 12f;
                }
            }

            SpawnTime++;
            NPC.velocity *= 0.9f;
        }
        else
        {
            NPC.dontTakeDamage = false;

            NPC.TargetClosest();

            NPCAimedTarget target = NPC.GetTargetData();

            const int WaitTime = 115;
            const int SpitTime = 43;

            if (NPC.life < NPC.lifeMax)
            {
                if (Time % 8 == 0)
                    NPC.life++;
            }
            else
            {
                if (ShootTime > WaitTime || (!target.Invalid && NPC.Distance(target.Center) < 500))
                {
                    if (ShootTime == (WaitTime + SpitTime / 2))
                    {
                        SoundEngine.PlaySound(SoundID.Item112 with { MaxInstances = 0, Pitch = 0.2f, PitchVariance = 0.1f }, NPC.Center);

                        if (!target.Invalid && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < Main.rand.Next(1, 4); i++)
                            {
                                Vector2 shotVel = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(6f, 8f);
                                Projectile blood = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, shotVel, ModContent.ProjectileType<BloodTear>(), 40, 0f);
                                blood.ai[1] = (int)BloodTear.Behavior.Fall;
                                blood.ai[2] = 1f;
                                blood.scale *= Main.rand.NextFloat(0.9f, 1f);
                            }
                        }
                    }

                    Vector2 squashIn = Vector2.Lerp(Vector2.One, new Vector2(1.3f, 0.7f), Utils.GetLerpValue(WaitTime + SpitTime / 5f, WaitTime + SpitTime / 3f, ShootTime, true));
                    Vector2 squashOut = Vector2.Lerp(Vector2.One, new Vector2(0.8f, 1.2f), MathF.Sqrt(Utils.GetLerpValue(SpitTime / 1.1f, SpitTime / 2f, ShootTime - WaitTime, true)));
                    DrawScale = Vector2.Lerp(squashIn, squashOut, Utils.GetLerpValue(SpitTime / 2.5f, SpitTime / 2f, ShootTime - WaitTime, true));

                    ShootTime++;

                    if (ShootTime >= WaitTime + SpitTime)
                        ShootTime = 0;
                }
            }
        }

        Time++;

        Lighting.AddLight(NPC.Center, Color.GhostWhite.ToVector3() * 0.3f);
        NPC.velocity = Vector2.Zero;

        NPC.FaceTarget();
    }

    public override void OnKill()
    {
        var flyParticle = LittleAngryBugParticle.RequestNew(NPC.Center, Vector2.Zero, Main.rand.Next(30, 60), Main.rand.NextFloat(0.5f, 1.5f));
        ParticleEngine.Particles.Add(flyParticle);

        for (int i = 0; i < Main.rand.Next(15, 20); i++)
        {
            Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.Blood, Main.rand.NextVector2Circular(5, 5), Scale: 1.5f);
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

    public override void FindFrame(int frameHeight)
    {
    }

    private Vector2 drawScale;
    public ref Vector2 DrawScale => ref drawScale;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;

        float spawnScale = Utils.GetLerpValue(0, 8, SpawnTime, true);

        float scale = 0.5f + NPC.GetLifePercent() * 0.5f * MathF.Sqrt(Utils.GetLerpValue(0, 18, SpawnTime, true));

        int growthFrame = scale < 0.7f || SpawnTime < 10 ? 1 : 0;

        if (growthFrame > 0)
            scale *= 1.5f;

        if (NPC.IsABestiaryIconDummy)
        {
            growthFrame = 0;
            scale = 1f;
            spawnScale = 1f;

            drawColor = Color.White;
            DrawScale = Vector2.One;
        }

        Rectangle frame = texture.Frame(3, 3, (int)Variant, 1 + growthFrame);
        Rectangle baseFrame = texture.Frame(3, 3, (int)Variant, 0);

        Vector2 origin = baseFrame.Size() * new Vector2(0.5f, 0.8f);
        Vector2 center = NPC.Center + new Vector2(0, 18).RotatedBy(NPC.rotation);

        spriteBatch.Draw(texture, center - screenPos, frame, drawColor * 1.1f, NPC.rotation, origin, DrawScale * scale * spawnScale * NPC.scale, 0, 0);
        spriteBatch.Draw(texture, center - screenPos, baseFrame, drawColor * 1.1f, NPC.rotation, origin, Vector2.Lerp(DrawScale, Vector2.One, 0.67f) * spawnScale * NPC.scale, 0, 0);

        return false;
    }
}