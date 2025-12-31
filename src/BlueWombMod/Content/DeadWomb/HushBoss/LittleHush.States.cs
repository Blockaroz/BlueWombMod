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
using Terraria.ID;
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

    public enum BossAttack
    {
        Idle,
        TearSpiral,
        RadialWaveTears,
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

    public void FaceTargetSpecial()
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
        AnimationFrame = 0;

        int distance = DistanceToFloor();

        if (distance > 20)
        {
            if (Time == 0)
            {
                NPC.velocity.Y = -3f;
            }

            NPC.velocity.Y += 0.4f;
            if (NPC.velocity.Y > 14)
            {
                NPC.velocity.Y = 14;
            }
        }
        else
        {
            NPC.velocity.Y *= 0.9f;

            NPC.velocity.Y += (distance - 30) * 0.05f;
        }

        if (Time > 90)
        {
            Music = MusicID.Boss2;

            NPC.dontTakeDamage = false;
            State = (int)BossState.PhaseShakingCrying;
            Attack = (int)BossAttack.Idle;
        }

        Time++;
    }

    public void Phase_Scared()
    {
        DrawOffset.X = Main.rand.NextFloat(-2, 2);
        AnimationFrame = 1;

        int distance = DistanceToFloor();
        NPC.velocity.Y += (distance - 21) * 0.01f;

        NPC.velocity += NPC.DirectionTo(HushSystem.HomePosition).SafeNormalize(Vector2.Zero) * 0.1f;

        NPC.velocity *= 0.96f;
    }
}
