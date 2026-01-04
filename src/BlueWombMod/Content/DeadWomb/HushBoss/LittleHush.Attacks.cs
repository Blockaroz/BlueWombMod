using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.DeadWomb.HushBoss.Minions.Flies;
using BlueWombMod.Content.DeadWomb.HushBoss.Projectiles;
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
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

public sealed partial class LittleHush : ModNPC
{
    public enum BossAttack
    {
        Idle,
        Teleport,
        TravelBloodVomit,
        TearSpiralWave,
        TearCircle,
        SpitFlies,
        SkyCrack,
        SkyFracture,
    }

    public void DoAttack()
    {
        switch (Attack)
        {
            case (int)BossAttack.TravelBloodVomit:
                Attack_TravelBloodVomit();
                break;

            case (int)BossAttack.TearSpiralWave:
                Attack_TearSpiralWave();
                break;

            case (int)BossAttack.SpitFlies:
                Attack_SpitFlies();
                break;
        }
    }

    public void EndAttack()
    {
        Time = 0;
        Attack = (int)BossAttack.Idle;
    }

    public int GetFlyStrength()
    {
        return 0;
    }

    public void FindTarget()
    {
        NPC.TargetClosest_WOF();
    }

    public void FindNewHomeSpot()
    {
        NPCAimedTarget target = NPC.GetTargetData();

        Vector2 wombCenter = HushSystem.WombPosition.ToWorldCoordinates();
        Vector2 offset;

        if (NPC.Distance(wombCenter) > 250)
        {
            offset = Vector2.Zero;
        }
        else
        {
            int offDir = NPC.Center.X > target.Center.X ? -1 : 1;
            offset = new Vector2(offDir * 300, 0).RotatedBy(Main.rand.NextBool().ToDirectionInt() * MathHelper.PiOver4);
        }

        // Dust.QuickDust(wombCenter + offset, Color.CornflowerBlue);
        SetHome(wombCenter + offset);
    }

    public void Attack_TravelBloodVomit()
    {
        const int ChargeTime = 33;
        const int PlacementTime = 60;
        const int TotalTime = ChargeTime + PlacementTime;

        if (Time == 0)
        {
            FindTarget();
        }

        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        FaceTarget();

        if (Main.netMode != NetmodeID.MultiplayerClient && Time == 0 && Main.rand.NextBool())
        {
            FindNewHomeSpot();
        }

        if (Time < ChargeTime)
        {
            NPC.velocity += NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 0.4f;
            NPC.velocity *= 0.92f - 0.5f * Utils.GetLerpValue(ChargeTime * 0.5f, ChargeTime, Time, true);

            DrawOffset += Main.rand.NextVector2Circular(5, 5) * MathF.Sin(Utils.GetLerpValue(0, ChargeTime, Time, true) * MathHelper.Pi);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.4f, 0.6f), Utils.GetLerpValue(0, ChargeTime, Time, true));
        }
        if (Time == ChargeTime)
        {
            NPC.velocity *= 0.5f;
            NPC.velocity -= NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 5f;

            SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.7f }, NPC.Center);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float distanceToTarget = NPC.Distance(target.Center);
                for (int i = 0; i < Main.rand.Next(8, 12); i++)
                {
                    Vector2 velocity = NPC.DirectionTo(target.Center) * Main.rand.NextFloat(2f, 3f);
                    velocity = velocity.RotatedByRandom(0.3f) * 2f - Vector2.UnitY * (Main.rand.NextFloat(3f, 5f) + distanceToTarget * 0.01f);
                    var tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<BloodTear>(), 40, 0f);
                    tear.ai[1] = 1;
                    tear.ai[2] = Main.rand.NextFloat(0.8f, 1.2f);
                }
            }
        }
        if (Time >= ChargeTime)
        {
            Vector2 initSquash = Vector2.Lerp(new Vector2(1.3f, 0.7f), new Vector2(0.8f, 1.4f), Utils.GetLerpValue(ChargeTime, ChargeTime + 6, Time, true));
            DrawScale = Vector2.Lerp(Vector2.One, initSquash, Utils.GetLerpValue(ChargeTime + 20, ChargeTime + 5, Time, true));

            float beginMove = Utils.GetLerpValue(ChargeTime + 10, TotalTime, Time, true);
            NPC.velocity *= 0.97f - 0.05f * beginMove;
            NPC.velocity += NPC.DirectionFrom(target.Center).SafeNormalize(Vector2.Zero) * 0.2f;
            NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * Utils.GetLerpValue(-20, 300, NPC.Distance(HomePosition)) * beginMove;

            if (Time < ChargeTime + 30)
            {
                AnimationFrame = 1;
            }
        }

        Time++;

        if (Time >= TotalTime)
        {
            EndAttack();
        }
    }

    public void Attack_TearSpiralWave()
    {
        const int ChargeTime = 30;
        const int WindDownTime = 110;
        
        float percent = Utils.GetLerpValue(1f, 0.75f, LifePercentForAttack, true);
        
        int WaveCount = 3 + (int)(6 * percent);
        int WaveTime = 16 - (int)(10 * percent);
        int TotalTime = ChargeTime + WindDownTime + WaveCount * WaveTime;

        NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.1f * Utils.GetLerpValue(20, 300, NPC.Distance(HomePosition));
        NPC.velocity *= 0.9f;

        if (Time == 0)
        {
            FindTarget();
        }

        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        FaceTarget();

        if (Time < ChargeTime)
        {
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.3f, 0.5f), Utils.GetLerpValue(ChargeTime / 2f, ChargeTime, Time, true));
        }
        else
        {
            float progressIn = Utils.GetLerpValue(0, 6, Time - ChargeTime, true);
            float progressOut = Utils.GetLerpValue(TotalTime, TotalTime - 10, Time + WindDownTime, true);
            float wobble = MathF.Sin(Time * 1.5f);
            DrawScale = Vector2.Lerp(Vector2.One, Vector2.Lerp(new Vector2(1.3f, 0.5f), new Vector2(0.8f + wobble * 0.1f, 1.3f - wobble * 0.1f), MathF.Sqrt(progressIn)), progressOut);
        }

        if (Time >= ChargeTime && Time < TotalTime - WindDownTime)
        {
            var localTime = Time - ChargeTime;

            if (localTime % WaveTime == 0)
            {
                float curl = localTime / ((WaveCount - 1f) * WaveTime);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    bool altWave = (int)(localTime / WaveTime) % 2 == 0;
                    int completeDirection = altWave ? 1 : -1;

                    int count = altWave ? 7 : 6;
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 direction = new Vector2(0, 6f).RotatedBy((float)i / count * MathHelper.TwoPi - curl * 1.5f * completeDirection);
                        Projectile tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, direction, ModContent.ProjectileType<HolyWaterTear>(), 20, 0.1f);
                        tear.ai[2] = 0.03f * completeDirection;
                        tear.timeLeft = 207;
                    }
                }
            }
        }

        Time++;

        if (Time >= TotalTime)
        {
            EndAttack();
        }
    }

    public record struct FlyStrengthProfile(int AttackFlyCount, int PooterFlyCount)
    {
        public int TotalCount => AttackFlyCount + PooterFlyCount;
    }

    public FlyStrengthProfile GetFlyProfile()
    {
        int attack = 5;
        int pooter = 3;

        return new FlyStrengthProfile(attack, pooter);
    }

    public void Attack_SpitFlies()
    {
        if (Time == 0)
        {
            FindTarget();
        }

        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        FaceTarget();

        FlyStrengthProfile flyProfile = GetFlyProfile();

        if (flyProfile.TotalCount < 1)
        {
            EndAttack();
            return;
        }

        const int ChargeTime = 19;
        int SpitTime = flyProfile.AttackFlyCount * 5 + 5;
        const int ReturnHomeTime = 50;
        int TotalTime = ChargeTime + SpitTime + ReturnHomeTime;

        Vector2 targetPosition = Vector2.Lerp(target.Center, HomePosition, Utils.GetLerpValue(ChargeTime, TotalTime, Time, true) * 0.6f + 0.4f);

        NPC.velocity *= 0.95f;
        NPC.velocity += NPC.DirectionTo(targetPosition).SafeNormalize(Vector2.Zero) * 0.15f;

        if (Time < ChargeTime)
        {
            NPC.velocity *= 1f - Utils.GetLerpValue(ChargeTime / 2f, ChargeTime, Time, true) * 0.3f;

            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1f, 1.3f), Utils.GetLerpValue(0, ChargeTime, Time, true));
        }

        Vector2 mouthPosition = NPC.Center + new Vector2(NPC.direction * 10f, -2f).RotatedBy(NPC.rotation);

        if (Time == ChargeTime)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

            for (int i = 0; i < Main.rand.Next(20, 30); i++)
            {
                Vector2 flyParticleVel = NPC.velocity + new Vector2(NPC.direction * Main.rand.NextFloat(5f, 17f), 2f).RotatedByRandom(0.6f);
                Dust flySmoke = Dust.NewDustPerfect(mouthPosition, DustID.Wraith, flyParticleVel, 100, Scale: Main.rand.NextFloat(2f, 4f));
                flySmoke.noGravity = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < flyProfile.PooterFlyCount; i++)
                {
                    NPC fly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), mouthPosition, ModContent.NPCType<PooterFly>());
                    fly.velocity = new Vector2(0, 5f).RotatedBy((float)i / flyProfile.PooterFlyCount * MathHelper.Pi * NPC.direction);
                }
            }
        }
        if (Time >= ChargeTime)
        {
            Vector2 initSquash = Vector2.Lerp(new Vector2(1f, 1.3f), new Vector2(1.5f, 0.7f), Utils.GetLerpValue(ChargeTime, ChargeTime + 2, Time, true));
            DrawScale = Vector2.Lerp(Vector2.One, initSquash, Utils.GetLerpValue(ChargeTime + SpitTime, ChargeTime + SpitTime / 2, Time, true));

            DrawOffset += Main.rand.NextVector2Circular(8, 8) * Utils.GetLerpValue(ChargeTime + SpitTime, ChargeTime, Time, true);

            if (Time < ChargeTime + SpitTime)
            {
                AnimationFrame = 1;

                Vector2 flyParticleVel = NPC.velocity + new Vector2(NPC.direction * Main.rand.NextFloat(3f, 12f), 1f).RotatedByRandom(0.6f);
                var flyParticle = LittleAngryBugParticle.RequestNew(mouthPosition, flyParticleVel * 0.6f, Main.rand.Next(80, 200), Main.rand.NextFloat(0.5f, 2f));
                ParticleEngine.Particles.Add(flyParticle);

                Dust flySmoke = Dust.NewDustPerfect(mouthPosition, DustID.Wraith, flyParticleVel, 100, Scale: Main.rand.NextFloat(1f, 3f));
                flySmoke.noGravity = true;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int flyTime = SpitTime / flyProfile.AttackFlyCount;
                    if (Time % flyTime == 0)
                    {
                        NPC fly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), mouthPosition, ModContent.NPCType<AttackFly>());
                        fly.velocity = new Vector2(NPC.direction * Main.rand.NextFloat(10f, 15f), Main.rand.NextFloat(-2f, 5f));
                    }
                }
            }
        }

        Time++;

        if (Time >= TotalTime)
        {
            EndAttack();
        }
    }
}