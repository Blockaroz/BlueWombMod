using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.DeadWomb.HushBoss.Minions.Flies;
using BlueWombMod.Content.DeadWomb.HushBoss.Projectiles;
using BlueWombMod.Content.Particles;
using Microsoft.Xna.Framework;
using ReLogic.Peripherals.RGB;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

public sealed partial class LittleHush : ModNPC
{
    public enum BossState
    {
        Spawning,
        Despawning,

        Idle,
        Teleport,

        StandUp,
        GrowWings,
        BigHushTime,

        TravelBloodVomit,
        TravelHomingSpray,
        SpitFlies,
        TearSpiralWave,
        TearSpiralStream,
        TearCircle,
        TearSplitters,
    }

    public void DoCurrentState()
    {
        switch (State)
        {
            case (int)BossState.Spawning:
                DoSpawn();
                break;

            case (int)BossState.Teleport:
                DoTeleport();
                break;

            case (int)BossState.StandUp:
                DoPhaseChange_StandUp();
                break;

            case (int)BossState.GrowWings:
                DoPhaseChange_GrowWings();
                break;

            case (int)BossState.TravelBloodVomit:
                Attack_TravelBloodVomit();
                break;

            case (int)BossState.TravelHomingSpray:
                Attack_TravelHomingSpray();
                break;

            case (int)BossState.SpitFlies:
                Attack_SpitFlies();
                break;

            case (int)BossState.TearSpiralWave:
                Attack_TearSpiralWave();
                break;

            case (int)BossState.TearSpiralStream:
                Attack_TearSpiralStream();
                break;

            case (int)BossState.TearCircle:
                Attack_TearCircle();
                break;

            case (int)BossState.TearSplitters:
                Attack_TearSplitters();
                break;
        }
    }

    public Vector2 HomePosition => new Point(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates();

    public void SetHome(Vector2 position)
    {
        Point pt = position.ToTileCoordinates();
        NPC.homeTileX = pt.X;
        NPC.homeTileY = pt.Y;
    }

    public int DistanceToFloor()
    {
        int x = (int)(NPC.Center.X / 16);
        int y = (int)(NPC.Center.Y / 16);
        for (int i = 0; i < 100; i++)
        {
            if (WorldGen.SolidOrSlopedTile(x, y + i))
            {
                return i;
            }
        }

        return -1;
    }

    public void FaceTarget()
    {
        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            NPC.direction = 0;
            return;
        }

        if (Math.Abs(target.Center.X - NPC.Center.X) < 80)
        {
            NPC.direction = 0;
        }
        else
        {
            NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
        }
    }

    public void EndAttack()
    {
        Time = 0;
        State = (int)BossState.Idle;
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
            AnimationFrame = (int)HushyPose.Crouched;

            NPC.velocity += NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 0.4f;
            NPC.velocity *= 0.92f - 0.5f * Utils.GetLerpValue(ChargeTime * 0.5f, ChargeTime, Time, true);

            DrawOffset += Main.rand.NextVector2Circular(5, 5) * MathF.Sin(Utils.GetLerpValue(0, ChargeTime, Time, true) * MathHelper.Pi);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.4f, 0.6f), Utils.GetLerpValue(0, ChargeTime, Time, true));
        }
        if (Time == ChargeTime)
        {
            NPC.velocity *= 0.5f;
            NPC.velocity -= NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 5f;

            SoundEngine.PlaySound(SoundID.Item111 with { Pitch = -0.2f }, NPC.Center);

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
                AnimationFrame = GetSpittingFrame();
            }
        }

        Time++;

        if (Time >= TotalTime)
        {
            EndAttack();
        }
    }

    public void Attack_TravelHomingSpray()
    {
        const int ChargeTime = 63;
        const int SprayTime = 50;
        const int PlacementTime = 100;
        const int TotalTime = ChargeTime + SprayTime + PlacementTime;

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
            AnimationFrame = (int)HushyPose.Crouched;

            NPC.velocity += NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 0.5f;
            NPC.velocity *= 0.92f - 0.5f * Utils.GetLerpValue(ChargeTime * 0.5f, ChargeTime, Time, true);

            DrawOffset += Main.rand.NextVector2Circular(5, 5) * MathF.Sin(Utils.GetLerpValue(0, ChargeTime, Time, true) * MathHelper.Pi);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.4f, 0.6f), Utils.GetLerpValue(0, ChargeTime, Time, true));
        }
        if (Time >= ChargeTime && Time < TotalTime - PlacementTime)
        {
            NPC.velocity *= 0.5f;
            NPC.velocity -= new Vector2(NPC.direction, 0) * 3f;

            if (Time % 4 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item111 with { Pitch = 0.8f }, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float distanceToTarget = NPC.Distance(target.Center);

                    Vector2 velocity = NPC.DirectionTo(target.Center) + new Vector2(NPC.direction, 0) + Main.rand.NextVector2Circular(4, 4);
                    var tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<SpoonBenderTear>(), 30, 0f);
                    tear.ai[0] = NPC.whoAmI + 1;
                }
            }

            if (NPC.collideX)
            {
                NPC.velocity.X *= -1f;
            }
        }
        if (Time >= ChargeTime)
        {
            Vector2 initSquash = Vector2.Lerp(new Vector2(0.5f, 1.5f), new Vector2(1.3f, 0.7f), Utils.GetLerpValue(ChargeTime, ChargeTime + 5, Time, true));
            DrawScale = Vector2.Lerp(Vector2.One, initSquash, Utils.GetLerpValue(ChargeTime + 12, ChargeTime + 6, Time, true));

            float beginMove = Utils.GetLerpValue(ChargeTime + 10, TotalTime, Time, true);
            NPC.velocity *= 0.97f - 0.05f * beginMove;
            NPC.velocity += NPC.DirectionFrom(target.Center).SafeNormalize(Vector2.Zero) * 0.2f;
            NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * Utils.GetLerpValue(-20, 300, NPC.Distance(HomePosition)) * beginMove;

            if (Time < TotalTime - PlacementTime)
            {
                AnimationFrame = (int)HushyPose.SpitCrouched;
            }
        }

        bool farEnough = NPC.Distance(HomePosition) > 200;
        if (Main.netMode != NetmodeID.MultiplayerClient && Time == TotalTime - 5 && farEnough)
        {
            TeleportTo(HomePosition);
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
        float inversePercent = Utils.GetLerpValue(0.9f, 0.3f, LifePercentForAttack, true);
        int attackFliesWanted = 2 + (int)(6 * inversePercent);
        int pootersWanted = (int)(3 * inversePercent);
        int attackers = NPC.CountNPCS(ModContent.NPCType<AttackFly>());
        int pooters = NPC.CountNPCS(ModContent.NPCType<PooterFly>());

        return new FlyStrengthProfile(attackFliesWanted - attackers, pootersWanted - pooters);
    }

    public bool SpitFliesCondition()
    {
        var flyProfile = GetFlyProfile();
        return flyProfile.TotalCount > 0;
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

        const int ChargeTime = 19;
        int SpitTime = flyProfile.AttackFlyCount * 7;
        const int ReturnHomeTime = 134;
        int TotalTime = ChargeTime + SpitTime + ReturnHomeTime;

        if (flyProfile.TotalCount < 1 && Time < ChargeTime)
        {
            EndAttack();
            return;
        }

        Vector2 targetPosition = Vector2.Lerp(target.Center, HomePosition, Utils.GetLerpValue(ChargeTime, TotalTime, Time, true) * 0.6f + 0.4f);

        NPC.velocity *= 0.95f;
        NPC.velocity += NPC.DirectionTo(targetPosition).SafeNormalize(Vector2.Zero) * 0.15f;

        if (Time < ChargeTime)
        {
            NPC.velocity *= 1f - Utils.GetLerpValue(ChargeTime / 2f, ChargeTime, Time, true) * 0.3f;

            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1f, 1.3f), Utils.GetLerpValue(0, ChargeTime, Time, true));
        }

        Vector2 mouthPosition = NPC.Center + new Vector2(NPC.direction * 10f, -1).RotatedBy(NPC.rotation);

        if (Time == ChargeTime)
        {
            AnimationFrame = (int)HushyPose.RaiseArmsStanding;

            SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

            for (int i = 0; i < Main.rand.Next(20, 30); i++)
            {
                Vector2 flyParticleVel = NPC.velocity + new Vector2(NPC.direction * Main.rand.NextFloat(5f, 17f), 2f).RotatedByRandom(0.6f);
                Dust flySmoke = Dust.NewDustPerfect(mouthPosition, DustID.Wraith, flyParticleVel, 50, Scale: Main.rand.NextFloat(2f, 4f));
                flySmoke.noGravity = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < flyProfile.PooterFlyCount; i++)
                {
                    NPC fly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), mouthPosition, ModContent.NPCType<PooterFly>());
                    fly.velocity = new Vector2(0, 9f).RotatedBy((float)i / flyProfile.PooterFlyCount * MathHelper.Pi * 1.5f * NPC.direction);
                }
            }
        }
        if (Time >= ChargeTime)
        {
            Vector2 initSquash = Vector2.Lerp(new Vector2(1f, 1.3f), new Vector2(1.5f, 0.7f), Utils.GetLerpValue(ChargeTime, ChargeTime + 2, Time, true));
            DrawScale = Vector2.Lerp(Vector2.One, initSquash, Utils.GetLerpValue(ChargeTime + SpitTime + 8, ChargeTime + SpitTime - 8, Time, true));

            DrawOffset += Main.rand.NextVector2Circular(8, 8) * Utils.GetLerpValue(ChargeTime + SpitTime, ChargeTime, Time, true);

            if (Time < ChargeTime + SpitTime)
            {
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

            if (Time < ChargeTime + SpitTime + 8)
            {
                AnimationFrame = (int)HushyPose.SpitCrouched;
            }
        }

        Time++;

        if (Time >= TotalTime)
        {
            EndAttack();
        }
    }

    public record struct TearSpiralProfile(int WaveCount, int TearsPerWave);

    public void Attack_TearSpiralWave()
    {
        float percent = Utils.GetLerpValue(1f, 0.75f, LifePercentForAttack, true);
        var tearProfile = new TearSpiralProfile(5 + (int)(6 * percent), 7);

        const int ChargeTime = 30;
        const int WindDownTime = 60;

        int WaveTime = 15 - (int)(5 * percent);
        int TotalTime = ChargeTime + WindDownTime + tearProfile.WaveCount * WaveTime;

        if (Time == 0)
        {
            FindTarget();
        }

        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.2f * Utils.GetLerpValue(10, 300, NPC.Distance(HomePosition));
        NPC.velocity *= 0.9f;

        if (Time < ChargeTime)
        {
            AnimationFrame = (int)HushyPose.Crouched;

            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.3f, 0.4f), Utils.GetLerpValue(ChargeTime / 2f, ChargeTime, Time, true));

            FaceTarget();
        }
        else
        {
            if (Time == ChargeTime)
            {
                NPC.direction = NPC.Center.X > target.Center.X ? -1 : 1;
            }

            float progressIn = Utils.GetLerpValue(0, 6, Time - ChargeTime, true);
            float progressOut = Utils.GetLerpValue(TotalTime, ChargeTime, Time + WindDownTime, true);
            float wobble = MathF.Sin(Utils.GetLerpValue(ChargeTime, TotalTime - 10, Time, true) * MathHelper.Pi);
            DrawScale = Vector2.Lerp(Vector2.One, Vector2.Lerp(new Vector2(1.3f, 0.4f), new Vector2(1.2f - wobble * 0.3f, 0.9f + wobble * 0.4f), MathF.Sqrt(progressIn)), progressOut);
            DrawOffset.Y = (DrawScale.Y - 1f) * -24f;
            DrawOffset += Main.rand.NextVector2Circular(4, 4) * wobble * progressOut;
        }

        if (Time >= ChargeTime && Time < TotalTime - WindDownTime)
        {
            var localTime = Time - ChargeTime;

            if (localTime % WaveTime == 0)
            {
                float curl = localTime / ((tearProfile.WaveCount - 1f) * WaveTime);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    bool altWave = (int)(localTime / WaveTime) % 3 == 0;
                    int completeDirection = NPC.direction;

                    for (int i = 0; i < tearProfile.TearsPerWave; i++)
                    {
                        Vector2 direction = new Vector2(4.5f + percent * 2f * Main.rand.NextFloat()).RotatedBy((float)i / tearProfile.TearsPerWave * MathHelper.TwoPi - curl * completeDirection);
                        Projectile tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, direction, ModContent.ProjectileType<HolyWaterTear>(), 20, 0.1f);
                        tear.ai[0] = NPC.whoAmI + 1;
                        tear.ai[1] = altWave ? 1 : 0;
                        tear.ai[2] = 0.02f * Main.rand.NextFloat(-1f, 1f) * completeDirection;
                        tear.timeLeft = altWave ? Main.rand.Next(120, 200) : 240;
                    }
                }
            }
        }
        else if (Time > TotalTime - WindDownTime)
        {
            FaceTarget();
        }

        Time++;

        if (Time >= TotalTime)
        {
            EndAttack();
        }
    }

    public void Attack_TearSpiralStream()
    {
        float percent = Utils.GetLerpValue(1f, 0.75f, LifePercentForAttack, true);
        var tearProfile = new TearSpiralProfile(5 - (int)(2 * percent), 12);

        const int ChargeTime = 30;
        const int WindDownTime = 110;

        int WaveTime = 10 - (int)(2 * percent);
        int TotalTime = ChargeTime + WindDownTime + tearProfile.WaveCount * WaveTime;

        if (Time == 0)
        {
            FindTarget();
        }

        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.2f * Utils.GetLerpValue(10, 300, NPC.Distance(HomePosition));
        NPC.velocity *= 0.9f;

        if (Time < ChargeTime)
        {
            FaceTarget();

            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.3f, 0.4f), Utils.GetLerpValue(ChargeTime / 2f, ChargeTime, Time, true));
        }
        else
        {
            if (Time == ChargeTime)
            {
                NPC.direction = NPC.Center.X > target.Center.X ? -1 : 1;
            }

            if (Time < TotalTime - WindDownTime / 2)
            {
                AnimationFrame = GetArmRaiseFrame();
                WingFrame = (int)HushyWingPose.Splayed;
            }

            float progressIn = Utils.GetLerpValue(0, 6, Time - ChargeTime, true);
            float progressOut = Utils.GetLerpValue(TotalTime - WindDownTime / 2f, TotalTime - WindDownTime / 1.5f, Time, true);
            float wobble = MathF.Sin(Time * 1.15f);
            DrawScale = Vector2.Lerp(Vector2.One, Vector2.Lerp(new Vector2(1.3f, 0.4f), new Vector2(0.8f, 1.3f - wobble * 0.05f), MathF.Sqrt(progressIn)), progressOut);
        }

        DrawOffset.Y = (DrawScale.Y - 1f) * -30f;

        if (Time >= ChargeTime && Time < TotalTime - WindDownTime)
        {
            var localTime = Time - ChargeTime;

            if (localTime % WaveTime == 0)
            {
                float curl = localTime / ((tearProfile.WaveCount - 1f) * WaveTime);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    bool altWave = (int)(localTime / WaveTime) % 3 == 0;
                    int completeDirection = NPC.direction;

                    for (int i = 0; i < tearProfile.TearsPerWave; i++)
                    {
                        Vector2 direction = new Vector2(-1, 6f + percent * Main.rand.NextFloat(0.5f)).RotatedBy((float)i / tearProfile.TearsPerWave * MathHelper.TwoPi);
                        Projectile tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, direction, ModContent.ProjectileType<HolyWaterTear>(), 20, 0.1f);
                        tear.ai[0] = NPC.whoAmI + 1;
                        tear.ai[1] = 0;
                        tear.ai[2] = -0.01f * completeDirection;
                        tear.timeLeft = altWave ? Main.rand.Next(120, 200) : 240;
                    }
                }
            }
        }
        else if (Time > TotalTime - WindDownTime)
        {
            FaceTarget();
        }

        Time++;

        if (Time >= TotalTime)
        {
            MiscTime = 30;
            EndAttack();
        }
    }

    public void Attack_TearCircle()
    {
        float percent = Utils.GetLerpValue(1f, 0.75f, LifePercentForAttack, true);
        var tearProfile = new TearSpiralProfile(4 + (int)(6 * percent), 6);

        const int ChargeTime = 40;
        const int WindDownTime = 50;

        int WaveTime = 16 - (int)(10 * percent);
        int TotalTime = ChargeTime + WindDownTime + tearProfile.WaveCount * WaveTime;

        if (Time == 0)
        {
            FindTarget();

            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.Distance(HomePosition) > 100 && Main.rand.NextBool())
            {
                TeleportTo(HomePosition);
            }
        }

        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.2f * Utils.GetLerpValue(10, 300, NPC.Distance(HomePosition));
        NPC.velocity *= 0.9f;

        if (Time < ChargeTime)
        {
            AnimationFrame = GetArmRaiseFrame();
            WingFrame = (int)HushyWingPose.Splayed;

            FaceTarget();

            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(0.6f, 1.3f), MathF.Sqrt(Utils.GetLerpValue(ChargeTime / 4f, ChargeTime, Time, true)));
        }
        else
        {
            if (Time == ChargeTime)
            {
                NPC.direction = NPC.Center.X > target.Center.X ? -1 : 1;
            }

            float progressIn = Utils.GetLerpValue(0, 6, Time - ChargeTime, true);
            float progressOut = Utils.GetLerpValue(TotalTime, TotalTime - 30, Time - ChargeTime + WindDownTime, true);
            DrawScale = Vector2.Lerp(Vector2.One, Vector2.Lerp(new Vector2(0.6f, 1.3f), new Vector2(1.3f, 0.8f), MathF.Sqrt(progressIn)), progressOut);
        }

        DrawOffset.Y = (DrawScale.Y - 1f) * -24f;

        if (Time >= ChargeTime && Time < TotalTime - WindDownTime)
        {
            AnimationFrame = (int)HushyPose.Crouched;
            WingFrame = (int)HushyWingPose.Closed;

            var localTime = Time - ChargeTime;

            if (localTime % WaveTime == 0)
            {
                float curl = localTime / ((tearProfile.WaveCount - 1f) * WaveTime);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    bool altWave = (int)(localTime / WaveTime) % 2 == 0;
                    int completeDirection = altWave ? 1 : -1;

                    int count = altWave ? tearProfile.TearsPerWave + 1 : tearProfile.TearsPerWave;
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 direction = new Vector2(0, 4f + percent * 2.5f).RotatedBy((float)i / count * MathHelper.TwoPi - curl * 1.5f * completeDirection);
                        Projectile tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, direction, ModContent.ProjectileType<HolyWaterTear>(), 20, 0.1f);
                        tear.ai[0] = NPC.whoAmI + 1;
                        tear.ai[2] = (0.03f + percent * 0.006f) * completeDirection;
                        tear.timeLeft = 207 - (int)(percent * 35);
                    }
                }
            }
        }
        else if (Time > TotalTime - WindDownTime)
        {
            FaceTarget();
        }

        Time++;

        if (Time >= TotalTime)
        {
            MiscTime = 150;
            EndAttack();
        }
    }

    public void Attack_TearSplitters()
    {
        const int ChargeTime = 23;
        const int PlacementTime = 120;
        const int TotalTime = ChargeTime + PlacementTime;

        if (Main.netMode != NetmodeID.MultiplayerClient && Time == 0 && Main.rand.NextBool())
        {
            FindNewHomeSpot();
        }

        NPC.direction = 0;

        if (Time < ChargeTime)
        {
            AnimationFrame = (int)HushyPose.Crouched;
            WingFrame = (int)HushyWingPose.Closed;

            NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.4f;
            NPC.velocity *= 0.92f - 0.5f * Utils.GetLerpValue(ChargeTime * 0.5f, ChargeTime, Time, true);

            DrawOffset += Main.rand.NextVector2Circular(5, 5) * MathF.Sin(Utils.GetLerpValue(0, ChargeTime, Time, true) * MathHelper.Pi);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.4f, 0.6f), Utils.GetLerpValue(0, ChargeTime, Time, true));
        }
        if (Time == ChargeTime)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int tearCount = Main.rand.Next(4, 8);
                float randRot = Main.rand.NextFloat(-1f, 1f);
                for (int i = 0; i < tearCount; i++)
                {
                    Vector2 velocity = new Vector2(0, 8f).RotatedBy((float)i / tearCount * MathHelper.TwoPi + randRot);
                    Projectile tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<HolyWaterTear>(), 20, 0.1f);
                    tear.ai[0] = NPC.whoAmI + 1;
                    tear.ai[1] = 2;
                    tear.timeLeft = 70;
                }
            }
        }
        if (Time >= ChargeTime)
        {
            Vector2 initSquash = Vector2.Lerp(new Vector2(1.3f, 0.7f), new Vector2(0.8f, 1.4f), Utils.GetLerpValue(ChargeTime, ChargeTime + 6, Time, true));
            DrawScale = Vector2.Lerp(Vector2.One, initSquash, Utils.GetLerpValue(ChargeTime + 20, ChargeTime + 5, Time, true));

            NPC.velocity *= 0.97f;

            if (Time < ChargeTime + 30)
            {
                AnimationFrame = GetArmRaiseFrame();
                WingFrame = (int)HushyWingPose.Splayed;
            }
        }

        Time++;

        if (Time >= TotalTime)
        {
            MiscTime = 63;
            EndAttack();
        }
    }
}