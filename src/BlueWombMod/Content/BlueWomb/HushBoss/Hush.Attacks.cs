using BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static BlueWombMod.Content.BlueWomb.HushBoss.LittleHush;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed partial class Hush : ModNPC
{
    public enum BossState
    {
        Spawning,
        Despawning,
        Death,

        Idle,
        Relocate,
        // Phase 1
        EyeRings,
        Salvos,
        // Phase 2
        FlyWheels,
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
            case (int)BossState.Idle:
                PickAttack();
                break;

            case (int)BossState.EyeRings:
                Attack_EyeRings();
                break;
        }
    }

    public static readonly NPCUtils.SearchFilter<Player> TargetOnlyPlayersInWomb = player => player.ZoneBlueWomb;

    public bool CheckForTarget()
    {
        var result = NPCUtils.SearchForTarget(NPC, NPCUtils.TargetSearchFlag.Players, Hush.TargetOnlyPlayersInWomb);
        NPC.target = result.NearestTargetIndex;

        if (NPC.GetTargetData().Invalid)
        {
            State = (int)BossState.Despawning;
            Time = 0;
            MiscTime = 0;

            return false;
        }

        return true;
    }

    public void EndAttack()
    {
        Time = 0;
        MiscTime = 0;
        State = (int)BossState.Idle;
    }

    public void Attack_EyeRings()
    {
        const int StartUpTime = 15;
        int WaveCount = 8;
        const int ShootTime = 24;
        int TotalTime = StartUpTime + WaveCount * ShootTime + 140;

        if (Time == 0)
        {
            NPC.FaceTarget();
        }

        if (Time < StartUpTime)
        {
            Renderer.Blink();

            float progress = Utils.GetLerpValue(0, StartUpTime * 0.8f, Time, true);
            Renderer.DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), MathF.Sqrt(progress));
            Renderer.DrawOffset.X += Main.rand.Next(-8, 8) * progress;

            Renderer.Mouth.Scale.Y *= 1f - MathF.Cbrt(progress) * 0.7f;
        }
        else if (Time < StartUpTime + WaveCount * ShootTime)
        {
            int localTime = (int)(Time - StartUpTime) % ShootTime;
            int eyeSide = (MathF.Floor((float)(Time - StartUpTime) / ShootTime) % 2) == 0 ? 1 : -1;
            int tearType = (int)Math.Floor((float)(Time - StartUpTime) / ShootTime) % 3;

            Vector2 unsquish = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), Utils.GetLerpValue(StartUpTime + 10, StartUpTime, Time, true));
            float wobble = MathF.Sin((float)localTime / ShootTime * MathHelper.Pi);
            Renderer.DrawScale = Vector2.Lerp(unsquish, new Vector2(0.9f, 1.1f), wobble);
            Renderer.DrawOffset += Main.rand.NextVector2Circular(2, 2) * MathF.Sqrt(wobble);

            if (localTime == 0)
            {
                Renderer.EyeStateLeft = HushRenderer.EyeState.Closed;
                Renderer.EyeStateRight = HushRenderer.EyeState.Closed;
                Renderer.Blink();
            }

            Color color = GlowingEyeTear.GetColorFromType(tearType) * Utils.GetLerpValue(ShootTime, ShootTime / 1.5f, localTime, true);
            if (eyeSide < 0)
                Renderer.GlowLeft(color);
            else
                Renderer.GlowRight(color);

            if (localTime == 5)
            {
                Vector2 position = NPC.Center + new Vector2(70 * eyeSide, -10).RotatedBy(Renderer.Face.Rotation + NPC.rotation) * Renderer.DrawScale;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float curl = Utils.GetLerpValue(0, WaveCount * ShootTime, Time - StartUpTime, true);
                    int count = (int)(10 + 8 * curl);
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 direction = new Vector2(0, -2.4f).RotatedBy((float)i / count * MathHelper.TwoPi + Time * 0.01f);
                        Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, direction, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                        glowTear.ai[0] = tearType;
                        glowTear.ai[1] = curl * 0.002f * eyeSide;
                    }
                }
            }
        }
        else
        {
        }

        if (Time >= TotalTime)
        {
            EndAttack();
            return;
        }

        Time++;
    }
}