using BlueWombMod.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

public sealed partial class LittleHush : ModNPC
{
    public enum BossState
    {
        Spawning,
        Despawning,
        BigHushTime,

        PhaseShakingCrying,
        PhaseBrave,
        PhaseAngelic
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

    public void DoSpawn()
    {
        NPC.velocity.Y = MathHelper.SmoothStep(0, 1, Utils.GetLerpValue(0, 120, Time, true)) * -3f;
        NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.5f;
        NPC.velocity *= 0.5f;

        AnimationFrame = 0;
        NPC.direction = 0;

        DrawOffset.X = MathF.Sin(MiscTime / 6f * MathHelper.TwoPi);

        FightModeStrength = Utils.GetLerpValue(30, 180, Time, true);

        if (Time > 100)
        {
            NPC.dontTakeDamage = false;
            State = (int)BossState.PhaseShakingCrying;
            Attack = (int)BossAttack.Idle;
            Time = 0;

            InitializedAttacks_Scared();

            return;
        }

        Time++;
    }

    public float LifePercentForAttack { get; private set; }

    private void PrepForAttackSelection()
    {
        LifePercentForAttack = NPC.GetLifePercent();
        Time = 0;
        MiscTime = 0;
    }

    public WeightedAttackPool<BossAttack> AttackPool = new WeightedAttackPool<BossAttack>();

    public void InitializedAttacks_Scared()
    {
        AttackPool.Clear();
        AttackPool.Add(BossAttack.TearSpiralWave, 1.0);
        AttackPool.Add(BossAttack.TearCircle, 1.0);
        AttackPool.Add(BossAttack.TravelBloodVomit, 0.5);
        AttackPool.Add(BossAttack.SpitFlies, 0.5, SpitFliesCondition);
    }

    public void Phase_Scared()
    {
        AnimationFrame = 0;

        if (Attack == (int)BossAttack.Idle)
        {
            IdleTime--;
            if (IdleTime <= 0)
            {
                IdleTime = 0;
                PrepForAttackSelection();

                if (AttackPool is null)
                {
                    InitializedAttacks_Scared();
                }

                Attack = (int)AttackPool.PickFromTop(2, 0.5);
            }
        }
        else
        {
            DoAttack();
        }

        DrawOffset.X += MathF.Sin(MiscTime / 6f * MathHelper.TwoPi);
    }
}
