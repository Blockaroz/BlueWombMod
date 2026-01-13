using BlueWombMod.Common.Graphics.Camera;
using BlueWombMod.Common.Utilities;
using BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;
using BlueWombMod.Content.BlueWomb.Tiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed partial class Hush : ModNPC
{
    public enum BossState
    {
        Spawning,
        Despawning,
        Death,
        PhaseChange,

        Idle,
        // Phase 1
        EyeRings,
        MouthSalvos,
        HomingVolleys,
        // Phase 2
        SinkRelocate,
        FlyWheels,
        GapRings,
        // Phase 3
        GaperTunnel,
        Continuum,
        // Phase 4
        Chase,
        Hemorrhage,
        // Phase 5
        TearBeams
    }

    public void DoCurrentState()
    {
        switch (State)
        {
            case (int)BossState.Spawning:
                DoSpawn();
                break;
            case (int)BossState.Despawning:
                DoDespawn();
                break;
            case (int)BossState.Death:
                DoDeath();
                break;
            case (int)BossState.PhaseChange:
                DoPhaseChange();
                break;
            case (int)BossState.Idle:
                CheckAndPickAttack();
                break;
            case (int)BossState.EyeRings:
                Attack_EyeRings();
                break;
            case (int)BossState.MouthSalvos:
                Attack_MouthSalvos();
                break;
            case (int)BossState.HomingVolleys:
                Attack_HomingVolleys();
                break;
            case (int)BossState.SinkRelocate:
                Interphase_SinkRelocate();
                break;
            case (int)BossState.FlyWheels:
                Attack_FlyWheels();
                break;
            case (int)BossState.GapRings:
                Attack_GapRings();
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

    public void DoSpawn()
    {
        const int PushTime = 95;
        const int BounceOutTime = 35;
        const int TotalTime = PushTime + BounceOutTime;

        NPC.dontTakeDamage = true;

        if (Time < PushTime)
        {
            if (Time == 0)
            {
                Vector2 epicenter = Vector2.Lerp(NPC.Center, HushSystem.WombPosition.ToWorldCoordinates(), 0.33f);
                Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(epicenter, Vector2.Zero, 5f, TotalTime, 2, uniqueID: "HushEntrance"));
                Main.instance.CameraModifiers.Add(new SpotFocusModifier(epicenter, PushTime, BounceOutTime));

                // Roar Sound
            }

            float pushProgress = Utils.GetLerpValue(0, PushTime, Time, true);
            float sqrtProgress = MathF.Sqrt(pushProgress);
            Renderer.HideMound = true;

            Renderer.DrawOffset.X = MathF.Sin(Time * 1.75f) * 6f * Utils.GetLerpValue(0.2f, 0.8f, MathF.Sin(pushProgress * 2f), true);
            Renderer.DrawScale = new Vector2(1f + sqrtProgress * 0.1f, 1f - sqrtProgress * 0.1f);
            Renderer.Mouth.Scale = new Vector2(0.9f + sqrtProgress * 0.2f, 1f - sqrtProgress * 0.2f);

            Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Closed;
            Renderer.EyeLeft.Offset.Y -= 7f;
            Renderer.EyeLeft.Rotation = 0.2f;
            Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Closed;
            Renderer.EyeRight.Offset.Y -= 7f;
            Renderer.EyeRight.Rotation = -0.2f;
            Renderer.Blink();
        }
        else if (Time <= TotalTime)
        {
            float bounceOut = MathF.Sin(Utils.GetLerpValue(PushTime, TotalTime, Time, true) * 8f) * Utils.GetLerpValue(TotalTime, PushTime + BounceOutTime / 3f, Time, true);
            Renderer.DrawScale = new Vector2(1f - bounceOut * 0.15f, 1f + bounceOut * 0.2f) * (0.4f + 0.6f * MathF.Cbrt(Utils.GetLerpValue(PushTime, PushTime + BounceOutTime / 2f, Time, true)));
            Renderer.Face.Scale *= 1f + MathF.Sqrt(1f - Utils.GetLerpValue(PushTime, PushTime + BounceOutTime / 3f, Time, true)) * 0.25f;
            Renderer.Face.Offset.Y = bounceOut * 12;

            if (Time == PushTime)
            {
                BreakRadius();

                Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 10f, 50, 3, "HushEntrance"));

                // Break Wall Sound
            }
        }

        NPC.Opacity = Utils.GetLerpValue(2, 8, Time, true);

        Time++;

        if (Time >= TotalTime + 61)
        {
            NPC.dontTakeDamage = false;

            Time = 0;
            MiscTime = 0;

            SetupAttackPool();
            State = (int)BossState.Idle;
        }
    }

    public void DoDespawn()
    {
        const int SinkTime = 20;
        const int FadeTime = 90;
        const int TotalTime = SinkTime + FadeTime;

        NPC.dontTakeDamage = true;

        if (Time < SinkTime)
        {
            float sinkTime = Utils.GetLerpValue(0, SinkTime, Time, true);

            Renderer.DrawScale = new Vector2(1f - MathF.Sin(sinkTime * MathHelper.Pi) * 0.2f, 1f + MathF.Sin(sinkTime * MathHelper.Pi) * 0.2f) * Utils.GetLerpValue(1.8f, 0.7f, sinkTime, true);
            Renderer.Face.Scale *= 1f + Utils.GetLerpValue(0.7f, 1.8f, sinkTime, true) * 0.5f;
            Renderer.Blink();
        }
        else
        {
            Renderer.HideMound = true;

            float bounceInTime = Utils.GetLerpValue(SinkTime, SinkTime + FadeTime / 4f, Time, true);

            Renderer.DrawScale = new Vector2(1f + MathF.Sqrt(1f - bounceInTime) * 0.2f, 1f - MathF.Sqrt(1f - bounceInTime) * 0.2f);
            Renderer.DrawOffset.X = MathF.Sin(Time * 1.75f) * 6f * bounceInTime;

            ScreenDarkness.frontColor = Color.Black;
            ScreenDarkness.screenObstruction = Utils.GetLerpValue(SinkTime + FadeTime / 3f, TotalTime, Time, true);

            if (Time > SinkTime + FadeTime / 2f)
                Renderer.Blink();
        }

        Time++;

        if (Time >= TotalTime)
        {
            NPC.active = false;
        }
    }

    public void DoDeath()
    {
        const int RoarTime = 24;
        const int FadeTime = 120;
        const int TotalTime = RoarTime + FadeTime;

        NPC.dontTakeDamage = true;

        if (Time == 0)
        {
            KillMyProjectiles();

            Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 5f, TotalTime, 3, "HushDeath"));
        }

        if (Time < RoarTime)
        {
            Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Closed;
            Renderer.EyeLeft.Offset.Y -= 7f;
            Renderer.EyeLeft.Rotation = 0.2f;
            Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Closed;
            Renderer.EyeRight.Offset.Y -= 7f;
            Renderer.EyeRight.Rotation = -0.2f;
            Renderer.Blink();

            float squeezeProgress = MathF.Sqrt(Utils.GetLerpValue(0, RoarTime, Time, true));
            Renderer.Mouth.Scale = new Vector2(1.1f + 0.2f * squeezeProgress, 0.7f - 0.2f * squeezeProgress);

            Renderer.DrawScale = new Vector2(1f + squeezeProgress * 0.2f, 1f - squeezeProgress * 0.1f);
        }
        else
        {
            if (Time == RoarTime)
            {
                Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 8f, FadeTime, 2, "HushDeath"));
            }

            float fadeProgress = Utils.GetLerpValue(TotalTime - FadeTime / 3f, TotalTime, Time, true);
            MoonlordDeathDrama.RequestLight(fadeProgress, NPC.Center);

            float bounceUpProgress = MathF.Sin(Utils.GetLerpValue(RoarTime, RoarTime + FadeTime / 5f, Time, true) * MathHelper.Pi);
            Renderer.DrawScale = Vector2.Lerp(new Vector2(1.2f, 0.9f), new Vector2(1f - bounceUpProgress * 0.15f, 1f + bounceUpProgress * 0.2f), Utils.GetLerpValue(RoarTime, RoarTime + 5, Time, true));

            // TODO: mouth frame
            Renderer.Mouth.Scale = new Vector2(1f, 1.2f);
        }

        Renderer.Face.Offset.Y -= 40 * Utils.GetLerpValue(RoarTime / 2f, RoarTime + FadeTime / 3f, Time, true);
        Renderer.DrawOffset.X = MathF.Sin(Time * 1.7f) * 4f * Utils.GetLerpValue(RoarTime / 2f, RoarTime + FadeTime / 5f, Time, true);
        Renderer.DrawOffset.Y += (Renderer.DrawScale.Y - 1f) * 10;

        Time++;

        if (Time >= TotalTime + 10)
        {
            NPC.life = 0;
            NPC.checkDead();

            //BreakRadius();
            //BuildLootBox();
        }
    }

    private WeightedAttackPool<BossState> AttackPool { get; set; } = new WeightedAttackPool<BossState>();

    public void SetupAttackPool()
    {
        AttackPool.Clear();
        AttackPool.Add(BossState.EyeRings, 0.5);
        AttackPool.Add(BossState.MouthSalvos, 0.5);
        AttackPool.Add(BossState.HomingVolleys, 0.2);
        AttackPool.Add(BossState.FlyWheels, 0.5, FlyWheelCondition);
    }

    public void CheckAndPickAttack()
    {
        Time = 0;
        MiscTime = 0;

        if (CheckPhaseChangeNeeded())
        {
            State = (int)BossState.PhaseChange;
            return;
        }

        if (CheckForTarget())
            State = (int)AttackPool.PickFromTop(2, 0.2);
        else
            State = (int)BossState.Despawning;

    }

    private bool CheckPhaseChangeNeeded()
    {
        if (State == (int)BossState.PhaseChange)
            return false;

        float percent = NPC.GetLifePercent();
        switch (Phase)
        {
            default:
            case 0:
                return percent < 0.8f;
            case 1:
                return percent < 0.6f;
            case 2:
                return percent < 0.4f;
            case 3:
                return percent < 0.2f;
        }
    }

    public void DoPhaseChange()
    {
        switch (Phase)
        {
            default:
                // How did you get here?
                State = (int)BossState.Idle;
                break;
            case 0:
                Phase++;
                State = (int)BossState.SinkRelocate;
                break;
        }
    }

    private void KillMyProjectiles()
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.type == ModContent.ProjectileType<GlowingEyeTear>() ||
                projectile.type == ModContent.ProjectileType<ContinuumTear>() ||
                projectile.type == ModContent.ProjectileType<SpoonBenderTear>() ||
                projectile.type == ModContent.ProjectileType<BloodTear>())
            {
                projectile.Kill();
            }
        }
    }
}