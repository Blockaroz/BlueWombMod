using BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
        // Phase 1
        EyeRings,
        MouthSalvos,
        LineVolleys,
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
            case (int)BossState.Idle:
                PickAttack();
                break;
            case (int)BossState.EyeRings:
                Attack_EyeRings();
                break;
            case (int)BossState.MouthSalvos:
                Attack_MouthSalvos();
                break;
            case (int)BossState.LineVolleys:
                Attack_LineVolleys();
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

    public int GlowTearType { get; private set => field = Math.Abs(value % 3); }

    public Vector2 TargetPosition { get; set; }

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

            Vector2 unsquish = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), Utils.GetLerpValue(StartUpTime + 10, StartUpTime, Time, true));
            float wobble = Utils.PingPongFrom01To010((float)localTime / ShootTime);
            Renderer.DrawScale = Vector2.Lerp(unsquish, new Vector2(0.9f, 1.1f), wobble);
            Renderer.DrawOffset += Main.rand.NextVector2Circular(2, 2) * MathF.Sqrt(wobble);

            if (localTime == 0)
            {
                GlowTearType++;

                Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Closed;
                Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Closed;
                Renderer.Blink();
            }

            Color color = GlowingEyeTear.GetColorFromType(GlowTearType) * Utils.GetLerpValue(ShootTime, ShootTime / 1.5f, localTime, true);

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
                        Vector2 direction = new Vector2(0, -2.4f - curl * 0.1f).RotatedBy((float)i / count * MathHelper.TwoPi + Time * 0.01f);
                        Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, direction, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                        glowTear.ai[0] = GlowTearType;
                        glowTear.ai[1] = curl * 0.003f * eyeSide;
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

    public void Attack_MouthSalvos()
    {
        const int StartUpTime = 18;
        const int SalvoTime = 12;
        const int TotalTime = StartUpTime + SalvoTime + 74;

        NPCAimedTarget target = NPC.GetTargetData();

        if (Time == 0)
        {
            NPC.FaceTarget();
        }

        float wobble = Math.Abs(MathF.Sin(Time * 0.08f)) * Utils.GetLerpValue(StartUpTime / 2f, StartUpTime, Time, true);
        Renderer.DrawScale = new Vector2(1f - wobble * 0.03f, 1f + wobble * 0.03f);

        const float faceOffsetAmount = 50f;
        if (Time < StartUpTime)
        {
            TargetPosition = target.Center;
            float moveFaceProgress = MathF.Cbrt(Utils.GetLerpValue(0, StartUpTime, Time, true));
            Renderer.Face.Rotation = Utils.AngleLerp(0, NPC.AngleTo(TargetPosition) - MathHelper.PiOver2, moveFaceProgress);
            Renderer.Face.Offset += new Vector2(0, faceOffsetAmount).RotatedBy(Renderer.Face.Rotation) * moveFaceProgress;

            Renderer.Mouth.Scale.Y *= 0.2f + 0.8f * MathF.Sqrt(Utils.GetLerpValue(StartUpTime / 2f, 0, Time, true));

            float chewProgress = Utils.GetLerpValue(StartUpTime / 2f, 0, Time, true);
            Renderer.EyeLeft.Scale = new Vector2(1f + chewProgress * 0.2f, 1f - chewProgress * 0.15f);
            Renderer.EyeRight.Scale = new Vector2(1f + chewProgress * 0.2f, 1f - chewProgress * 0.15f);
        }
        else
        {
            Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Squint;
            Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Squint;

            float faceReturnProgress = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(TotalTime / 1.1f, TotalTime / 2f, Time, true));
            Renderer.Face.Rotation = Utils.AngleLerp(0, NPC.AngleTo(TargetPosition) - MathHelper.PiOver2, faceReturnProgress);
            Renderer.Face.Offset += new Vector2(0, faceOffsetAmount).RotatedBy(Renderer.Face.Rotation) * faceReturnProgress;

            float spitProgress = MathF.Sin(Utils.GetLerpValue(0, SalvoTime + 5, Time - StartUpTime, true) * MathHelper.Pi);
            Renderer.EyeLeft.Scale.Y += spitProgress * 0.2f;
            Renderer.EyeRight.Scale.Y += spitProgress * 0.2f;
            Renderer.Mouth.Scale.X *= 0.8f;
            Renderer.Mouth.Scale.Y *= (MathF.Sqrt(Utils.GetLerpValue(0, SalvoTime, Time - StartUpTime, true)) + spitProgress * 0.67f);

            if (Time < StartUpTime + SalvoTime)
            {
                if (Time == StartUpTime + 1)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        GlowTearType = Main.rand.Next(3);
                        NPC.netUpdate = true;

                        const float MouthPosOff = faceOffsetAmount * 1.8f;
                        const int SalvoWidth = 6;
                        for (int i = -SalvoWidth; i <= SalvoWidth; i++)
                        {
                            float xOff = MathF.Pow(Math.Abs((float)i) / SalvoWidth, 0.8f) * Math.Sign(i);

                            Vector2 position = NPC.Center + new Vector2(xOff * 40, MouthPosOff - Math.Abs(xOff) * 10).RotatedBy(Renderer.Face.Rotation);
                            Vector2 direction = Vector2.UnitY.RotatedBy(Renderer.Face.Rotation - xOff * 0.1f);

                            float speed = 4.5f - MathF.Pow(Math.Abs(xOff) * 0.9f, 2.22222222222222222f);
                            Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, direction * speed, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                            glowTear.ai[0] = GlowTearType;
                        }
                    }
                }
            }
        }

        Renderer.EyeLeft.Offset.Y += (Renderer.EyeLeft.Scale.Y - 1f) * -5f;
        Renderer.EyeRight.Offset.Y += (Renderer.EyeRight.Scale.Y - 1f) * -5f;
        Renderer.Mouth.Offset.Y += (Renderer.Mouth.Scale.Y - 1f) * 7f;

        if (Time >= TotalTime + 50)
        {
            EndAttack();
            return;
        }

        Time++;
    }

    public void Attack_LineVolleys()
    {
        const int ChargeTime = 11;
        const int VolleyTime = 16;
        const int TotalTime = ChargeTime + VolleyTime;
        const int Repeats = 3;

        float localTime = Time % TotalTime;

        if (Time == 0)
        {
            GlowTearType++;
        }

        if (localTime < ChargeTime)
        {

        }
        else
        {
            if (localTime == ChargeTime)
            {

            }
        }

        if (Time >= TotalTime * Repeats)
        {
            EndAttack();
            return;
        }

        Time++;
    }

    public void Attack_GaperTunnel()
    {
        const int StartUpTime = 15;
        const int GaperCount = 20;
        const int TimePerGaper = 6;
        const int TotalTime = StartUpTime + GaperCount * TimePerGaper + 5;

        if (Time < StartUpTime)
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