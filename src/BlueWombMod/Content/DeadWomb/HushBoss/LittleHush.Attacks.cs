using BlueWombMod.Content.DeadWomb.HushBoss.Projectiles;
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
        Travel,
        TravelAndSpit,
        TearSpiral,
        RadialWaveTears,
        SkyCrack,
        SkyFracture,
    }

    public void DoAttack()
    {
        switch (Attack)
        {
            case (int)BossAttack.TravelAndSpit:
                Attack_TravelAndSpit();
                break;
            case (int)BossAttack.TearSpiral:
                Attack_TearSpiral();
                break;
        }
    }

    public void Attack_TravelAndSpit()
    {
        const int ChargeTime = 33;
        const int PlacementTime = 60;
        const int TotalTime = ChargeTime + PlacementTime;

        if (Time == 0)
        {
            NPC.TargetClosest();
        }

        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        FaceTarget();

        if (Main.netMode != NetmodeID.MultiplayerClient && Time == 0)
        {
            Vector2 wombCenter = HushSystem.WombPosition.ToWorldCoordinates();
            Vector2 offset;

            if (NPC.Distance(wombCenter) > 250)
            {
                offset = Vector2.Zero;
            }
            else
            {
                int offDir = NPC.Center.X > target.Center.X ? -1 : 1;
                offset = new Vector2(offDir * 300, 0).RotatedBy(Main.rand.Next(-1, 2) / 3f * MathHelper.Pi);
            }

            SetHome(wombCenter + offset);
            Dust.QuickDustLine(wombCenter, HomePosition, 100, Color.Red);
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
            NPC.velocity -= NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 7f;

            SoundEngine.PlaySound(SoundID.NPCDeath19, NPC.Center);
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
            DrawScale = Vector2.Lerp(Vector2.One, Vector2.Lerp(new Vector2(1.3f, 0.7f), new Vector2(0.8f, 1.4f), Utils.GetLerpValue(ChargeTime, ChargeTime + 6, Time, true)), MathF.Sqrt(Utils.GetLerpValue(ChargeTime + 30, ChargeTime + 10, Time, true)));

            NPC.velocity *= 0.98f - 0.1f * Utils.GetLerpValue(ChargeTime, TotalTime, Time, true);
            NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.4f * Utils.GetLerpValue(0, 300, NPC.Distance(HomePosition));

            if (Time < ChargeTime + 30)
            {
                AnimationFrame = 1;
            }
        }

        if (Time > TotalTime)
        {
            Attack = (int)BossAttack.Idle;
        }

        Time++;
    }

    public void Attack_TearSpiral()
    {
        const int ChargeTime = 30;
        const int WindDownTime = 90;
        float percent = Utils.GetLerpValue(1f, 0.75f, LifePercentForAttack, true);
        int WaveCount = 3 + (int)(6 * percent);
        int WaveTime = 18 - (int)(10 * percent);
        int TotalTime = ChargeTime + WindDownTime + WaveCount * WaveTime;

        NPC.velocity += NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 0.1f;
        NPC.velocity *= 0.9f;

        if (Time == 0)
        {
            NPC.TargetClosest();
            FaceTarget();
        }
        
        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            // Yada yada
        }

        if (Time >= ChargeTime && Time < TotalTime - WindDownTime)
        {
            var localTime = Time - ChargeTime;

            if (localTime % WaveTime == 0)
            {
                float curl = localTime / ((WaveCount - 1f) * WaveTime);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int completeDirection = NPC.Center.X < target.Center.X ? 1 : -1;
                    const int count = 5;
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 direction = new Vector2(0, 5f).RotatedBy((float)i / count * MathHelper.TwoPi - curl * 1.5f * completeDirection);
                        Projectile tear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, direction, ModContent.ProjectileType<HolyWaterTear>(), 20, 0.1f);
                        tear.ai[2] = 0.05f * completeDirection;
                        tear.timeLeft = 85;
                    }
                }
            }
        }

        Time++;

        if (Time >= TotalTime)
        {
            Attack = (int)BossAttack.Idle;
        }
    }
}