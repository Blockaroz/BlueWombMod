using BlueWombMod.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static tModPorter.ProgressUpdate;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

public sealed partial class LittleHush : ModNPC
{
    public enum BossPhase
    {
        Scared,
        Standing,
        Angel
    }

    public int LastAttack { get; private set; }

    private Vector2 teleportPos;
    public ref Vector2 TeleportPosition => ref teleportPos;

    public void TeleportTo(Vector2 position)
    {
        LastAttack = (int)State;
        State = (int)BossState.Teleport;
        teleportPos = position;
        MiscTime = 0;
    }

    public void DoTeleport()
    {
        NPC.dontTakeDamage = true;

        const int TeleportCharge = 10;
        const int TeleportWindDown = 10;

        NPC.velocity *= 0.7f;
        NPC.rotation *= 0.7f;

        Dust smoke = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(30, 40), DustID.Wraith, Vector2.UnitY * -3f, Alpha: 100, Scale: Main.rand.NextFloat(0.5f, 2f));
        smoke.noGravity = true;

        if (MiscTime < TeleportCharge)
        {
            float teleportInProgress = Utils.GetLerpValue(0, TeleportCharge, MiscTime, true);
            NPC.Opacity = Utils.GetLerpValue(TeleportCharge - 2, TeleportCharge / 2, MiscTime, true);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1f - MathF.Sqrt(teleportInProgress), 1f + teleportInProgress * 2f), MathF.Sqrt(teleportInProgress));
            DrawOffset.Y -= (DrawScale.Y - 1f) * 20;
        }
        if (MiscTime == TeleportCharge)
        {
            NPC.Center = TeleportPosition;
        }
        if (MiscTime >= TeleportCharge)
        {
            AnimationFrame = GetArmRaiseFrame();

            float teleportOutProgress = Utils.GetLerpValue(TeleportWindDown, 0, MiscTime - TeleportCharge, true);
            NPC.Opacity = Utils.GetLerpValue(TeleportWindDown / 2, TeleportWindDown - 2, MiscTime - TeleportCharge, true);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(2f - teleportOutProgress, teleportOutProgress), teleportOutProgress);
            DrawOffset.Y -= (DrawScale.Y - 1f) * 20;
        }

        MiscTime++;

        if (MiscTime >= TeleportCharge + TeleportWindDown)
        {
            NPC.dontTakeDamage = false;

            State = LastAttack;
            LastAttack = -1;
            MiscTime = 0;
        }
    }

    public void DoSpawn()
    {
        NPC.velocity *= 0.5f;

        AnimationFrame = 0;
        NPC.direction = 0;

        if (Time == 0)
        {
            TeleportTo(HomePosition);
        }

        FightModeStrength = Utils.GetLerpValue(0, 20, Time, true);

        if (Time > 20)
        {
            NPC.dontTakeDamage = false;
            Phase = (int)BossPhase.Scared;

            InitializeAttacks_Scared();

            PrepareForAttack();
            State = (int)AttackPool.PickFromTop(AttackPool.Count, 0);

            AddBossArmorFactor = 8000;

            return;
        }

        Time++;
    }

    public float LifePercentForAttack { get; private set; }

    private void PrepareForAttack()
    {
        LifePercentForAttack = NPC.GetLifePercent();
        Time = 0;
        VisualTime = 0;
    }

    public WeightedAttackPool<BossState> AttackPool = new WeightedAttackPool<BossState>();

    public void InitializeAttacks_Scared()
    {
        AttackPool.Clear();
        AttackPool.Add(BossState.TearSpiralWave, 1.0);
        AttackPool.Add(BossState.TravelBloodVomit, 0.5);
        AttackPool.Add(BossState.TearCircle, 1.0);
        AttackPool.Add(BossState.TravelHomingSpray, 0.1);
        AttackPool.Add(BossState.SpitFlies, 0.2, SpitFliesCondition);
    }

    public void Phase_Scared()
    {
        AnimationFrame = (int)HushyPose.Crouched;

        if (State != (int)BossState.StandUp)
        {
            if (NPC.GetLifePercent() < 0.6f)
            {
                NPC.dontTakeDamage = true;

                if (State == (int)BossState.Idle)
                {
                    State = (int)BossState.StandUp;
                    Time = 0;
                    MiscTime = 0;
                }
            }
            else
            {
                DrawOffset.X += MathF.Sin(VisualTime / 5f * MathHelper.TwoPi) * 1.5f;
            }
        }

        if (State == (int)BossState.Idle)
        {
            if (MiscTime > 0)
            {
                MiscTime--;

                NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.2f * Utils.GetLerpValue(10, 300, NPC.Distance(HomePosition));
                NPC.velocity *= 0.9f;
            }
            else
            {
                PrepareForAttack();
                State = (int)AttackPool.PickFromTop(3, 0.15);
            }
        }
    }

    public void DoPhaseChange_StandUp()
    {
        NPC.dontTakeDamage = true;
        NPC.direction = 0;
        NPC.velocity *= 0.9f;

        const int WakeTime = 22;
        const int GainFootingTime = 17;

        Phase = (int)BossPhase.Standing;

        if (Time < 30)
        {
            AnimationFrame = (int)HushyPose.Crouched;

            float progress = Utils.GetLerpValue(0, WakeTime, Time, true);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.4f, 0.6f), progress);
            DrawOffset.X += MathF.Sin(VisualTime / 6f * MathHelper.TwoPi) * 4f * progress;
            DrawOffset.Y -= (DrawScale.Y - 1f) * 24f;
        }
        else
        {
            AnimationFrame = (int)HushyPose.Standing;

            float progress = Utils.GetLerpValue(WakeTime, WakeTime + GainFootingTime, Time, true);
            DrawScale = Vector2.Lerp(Vector2.One, Vector2.Lerp(new Vector2(1.4f, 0.6f), new Vector2(0.7f, 1.5f), MathF.Sqrt(Utils.GetLerpValue(0f, 0.6f, progress, true))), MathF.Sqrt(1f - progress));
            DrawOffset.Y -= (DrawScale.Y - 1f) * 30f;
        }

        if (Time > WakeTime + GainFootingTime + 2)
        {
            NPC.dontTakeDamage = false;

            InitializeAttacks_Standing();

            PrepareForAttack();
            State = (int)AttackPool.PickFromTop(AttackPool.Count, 0);
        }

        Time++;
    }

    public void InitializeAttacks_Standing()
    {
        AttackPool.Clear();
        AttackPool.Add(BossState.TearSpiralWave, 1.0);
        AttackPool.Add(BossState.TearCircle, 1.0);
        AttackPool.Add(BossState.TravelBloodVomit, 0.3);
        AttackPool.Add(BossState.TravelHomingSpray, 0.3);
        AttackPool.Add(BossState.SpitFlies, 0.5, SpitFliesCondition);
        AttackPool.Add(BossState.TearSplitters, 0.2);
        AttackPool.Add(BossState.TearSpiralStream, 1.0);
    }

    public void Phase_Standing()
    {
        AnimationFrame = (int)HushyPose.Standing;

        if (State != (int)BossState.GrowWings)
        {
            if (NPC.GetLifePercent() < 0.4f)
            {
                NPC.dontTakeDamage = true;

                if (State == (int)BossState.Idle)
                {
                    State = (int)BossState.GrowWings;
                    Time = 0;
                    MiscTime = 0;
                }
            }
            else
            {
                DrawOffset.Y += MathF.Sin(VisualTime / 120f * MathHelper.TwoPi) * 2f;
            }
        }

        if (State == (int)BossState.Idle)
        {
            if (MiscTime > 0)
            {
                MiscTime--;

                NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.2f * Utils.GetLerpValue(10, 300, NPC.Distance(HomePosition));
                NPC.velocity *= 0.9f;
            }
            else
            {
                PrepareForAttack();
                State = (int)AttackPool.PickFromTop(3, 0.1);
            }
        }
    }

    public void DoPhaseChange_GrowWings()
    {
        NPC.dontTakeDamage = true;
        NPC.direction = 0;
        NPC.velocity *= 0.9f;

        const int WakeTime = 22;
        const int SpreadWingsTime = 37;

        Phase = (int)BossPhase.Angel;

        if (Time < 30)
        {
            AnimationFrame = (int)HushyPose.Crouched;
            WingFrame = (int)HushyWingPose.Closed;

            float progress = Utils.GetLerpValue(0, WakeTime, Time, true);
            DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.4f, 0.6f), progress);
            DrawOffset.X += MathF.Sin(VisualTime / 6f * MathHelper.TwoPi) * 4f * progress;
            DrawOffset.Y -= (DrawScale.Y - 1f) * 24f;
        }
        else
        {
            AnimationFrame = (int)HushyPose.RaiseArmsStanding;
            WingFrame = (int)HushyWingPose.Splayed;

            float progress = Utils.GetLerpValue(WakeTime, WakeTime + SpreadWingsTime, Time, true);
            DrawScale = Vector2.Lerp(Vector2.One, Vector2.Lerp(new Vector2(1.4f, 0.6f), new Vector2(0.7f, 1.2f), MathF.Sqrt(Utils.GetLerpValue(0f, 0.6f, progress, true))), MathF.Sqrt(1f - progress));
            DrawOffset.Y -= (DrawScale.Y - 1f) * 30f;
        }

        if (Time > WakeTime + SpreadWingsTime + 2)
        {
            NPC.dontTakeDamage = false;

            InitializeAttacks_Angel();

            PrepareForAttack();
            State = (int)AttackPool.PickFromTop(AttackPool.Count, 0);
        }

        Time++;
    }

    public void InitializeAttacks_Angel()
    {
        AttackPool.Clear();
        AttackPool.Add(BossState.TearSpiralWave, 1.0);
        AttackPool.Add(BossState.TearCircle, 1.0);
        AttackPool.Add(BossState.TravelBloodVomit, 0.3);
        AttackPool.Add(BossState.TravelHomingSpray, 0.3);
        AttackPool.Add(BossState.TearSplitters, 0.2);
        AttackPool.Add(BossState.SpitFlies, 0.5, SpitFliesCondition);
        AttackPool.Add(BossState.TearSpiralStream, 0.3);
        AttackPool.Add(BossState.TearSpiralStream, 1.0);
    }

    public void Phase_Angel()
    {
        AnimationFrame = (int)HushyPose.Standing;

        if (State == (int)BossState.Idle)
        {
            if (MiscTime > 0)
            {
                MiscTime--;

                FindTarget();
                NPCAimedTarget target = NPC.GetTargetData();
                Vector2 targetPos = Vector2.Lerp(HomePosition, target.Center, Utils.GetLerpValue(0, 60, MiscTime, true));

                NPC.velocity += NPC.DirectionTo(targetPos).SafeNormalize(Vector2.Zero) * 0.2f;
                NPC.velocity *= 0.93f;
            }
            else
            {
                PrepareForAttack();
                State = (int)AttackPool.PickFromTop(3, 0.1);
            }
        }
    }
}
