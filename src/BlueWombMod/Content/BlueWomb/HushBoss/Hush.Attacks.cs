using BlueWombMod.Common.Graphics;
using BlueWombMod.Common.Graphics.Camera;
using BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;
using BlueWombMod.Content.Particles;
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
                PickAttack();
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
            NPC.FaceTarget();

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
                        glowTear.localAI[0] = GlowTearType;
                        glowTear.ai[1] = curl * 0.003f * eyeSide;
                    }
                }
            }
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
            NPC.FaceTarget();

        float wobble = Math.Abs(MathF.Sin(Time * 0.08f)) * Utils.GetLerpValue(StartUpTime / 2f, StartUpTime, Time, true);
        Renderer.DrawScale = new Vector2(1f - wobble * 0.03f, 1f + wobble * 0.03f);

        const float faceOffsetAmount = 50f;
        if (Time < StartUpTime)
        {
            if (!target.Invalid)
                TargetPosition = target.Center;

            float moveFaceProgress = MathF.Sqrt(Utils.GetLerpValue(0, StartUpTime, Time, true));
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
            float spitProgress = MathF.Sin(Utils.GetLerpValue(0, SalvoTime + 5, Time - StartUpTime, true) * MathHelper.Pi);

            Renderer.Face.Rotation = Utils.AngleLerp(0, NPC.AngleTo(TargetPosition) - MathHelper.PiOver2, faceReturnProgress);
            Renderer.Face.Offset += new Vector2(0, faceOffsetAmount - spitProgress * 15).RotatedBy(Renderer.Face.Rotation) * MathF.Sqrt(faceReturnProgress);

            Renderer.EyeLeft.Scale.Y += spitProgress * 0.2f;
            Renderer.EyeRight.Scale.Y += spitProgress * 0.2f;
            Renderer.Mouth.Scale.X *= 0.8f;
            Renderer.Mouth.Scale.Y *= (MathF.Sqrt(Utils.GetLerpValue(0, SalvoTime, Time - StartUpTime, true)) + spitProgress * 0.67f);

            Renderer.DrawOffset.X += Main.rand.NextFloat(-5f, 5f) * Utils.GetLerpValue(SalvoTime + 5, 0, Time - StartUpTime, true);

            if (Time < StartUpTime + SalvoTime)
            {
                if (Time == StartUpTime + 1)
                {
                    SoundEngine.PlaySound(SoundID.Item112 with { Pitch = -0.2f }, NPC.Center);

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
                            glowTear.ai[0] = 1;
                            glowTear.localAI[0] = GlowTearType;
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

    public void Attack_HomingVolleys()
    {
        const int ChargeTime = 20;
        const int VolleyTime = 30;
        const int TotalTime = ChargeTime + VolleyTime;
        const int Repeats = 3;

        float localTime = Time % TotalTime;

        NPCAimedTarget target = NPC.GetTargetData();

        if (Time == 0)
        {
            GlowTearType++;
            NPC.FaceTarget();

            if (!target.Invalid)
                TargetPosition = target.Center;
        }

        if (!target.Invalid)
            TargetPosition = Vector2.Lerp(TargetPosition, target.Center, 0.05f);

        if (Time < TotalTime * Repeats)
        {
            if (localTime < ChargeTime)
            {
                float chargeProgress = MathF.Sqrt(Utils.GetLerpValue(0, ChargeTime, localTime, true));
                Renderer.MouthState = HushRenderer.MouthAnimationState.Chewing;
                Renderer.Mouth.Scale = new Vector2(1.2f - chargeProgress * (0.4f + Main.rand.NextFloat(0.3f)), 0.5f + chargeProgress * (0.7f + Main.rand.NextFloat(0.2f)));

                Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Squint;
                Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Squint;

                Renderer.DrawScale *= new Vector2(1f + chargeProgress * 0.15f, 1f - chargeProgress * 0.2f);
                Renderer.DrawOffset.X += Main.rand.NextFloat(-4f, 4f) * chargeProgress;

                if (localTime > ChargeTime / 2)
                    Renderer.Blink();
            }
            else
            {
                float spitProgress = MathF.Sqrt(Utils.GetLerpValue(ChargeTime, ChargeTime + VolleyTime, localTime, true));
                Renderer.MouthState = HushRenderer.MouthAnimationState.Wide;
                Renderer.Mouth.Scale = new Vector2(0.8f + MathF.Pow(spitProgress, 6f) * 0.25f, 0.7f + MathF.Sin(spitProgress * MathHelper.Pi) * 0.7f);
                Renderer.Mouth.Offset.Y += (Renderer.Mouth.Scale.Y - 1f) * 7f;

                Renderer.DrawScale *= new Vector2(1f - MathF.Sin(spitProgress * MathHelper.Pi) * 0.2f, 1f + MathF.Sin(spitProgress * MathHelper.Pi) * 0.2f);

                if (localTime == ChargeTime)
                {
                    Vector2 mouthPos = NPC.Center + new Vector2(0, 70).RotatedBy(NPC.rotation);

                    SoundEngine.PlaySound(SoundID.Item111 with { Pitch = -0.2f }, mouthPos);

                    var splat = HomingTearPopParticle.RequestNew(mouthPos + Vector2.UnitY * 10f, 15, Main.rand.NextFloat(2f, 3f), MathHelper.Pi + Main.rand.NextFloat(-0.2f, 0.2f));
                    ParticleEngine.Particles.Add(splat);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 direction = -Vector2.UnitY.RotatedBy(i / 5f * MathHelper.TwoPi);
                             
                            Vector2 velocity = direction.RotatedByRandom(0.2f) * 2f + Vector2.UnitY * 4f;
                            Projectile homing = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), mouthPos, velocity, ModContent.ProjectileType<SpoonBenderTear>(), 40, 0f);
                            homing.ai[0] = NPC.whoAmI + 1;
                            homing.scale *= Main.rand.NextFloat(0.9f, 1.12f);
                        }
                    }
                }
            }

            Renderer.DrawOffset.Y += (Renderer.DrawScale.Y - 1f) * 70f;
        }

        if (Time >= TotalTime * Repeats + 22)
        {
            EndAttack();
            return;
        }

        Time++;
    }

    public void Interphase_SinkRelocate()
    {
        const int SinkTime = 100;
        const int ConfirmationTime = 10;

        if (Time < SinkTime)
        {
            NPC.dontTakeDamage = true;
            NPC.velocity *= 0.5f;
            SetHome(HushSystem.WombPosition.ToWorldCoordinates() - new Vector2(0, 50));

            Renderer.SinkDown();

            Time++;
            MiscTime = 0;

            if (Time == SinkTime - 1)
            {
                Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 10f, 50, 3, "HushJump"));
            }
        }
        else
        {
            NPC.dontTakeDamage = false;

            Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Squint;
            Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Squint;
            Renderer.Mouth.Scale = new Vector2(0.6f, 1f);

            if (NPC.Distance(HomePosition) < 20)
            {
                NPC.velocity *= 0.5f;
                NPC.Center = Vector2.Lerp(NPC.Center, HomePosition, 0.1f);

                Time++;
            }
            else
            {
                NPC.velocity = NPC.DirectionTo(HomePosition).SafeNormalize(Vector2.Zero) * 1.67f;
            }

            float wobble = Math.Abs(MathF.Sin((MiscTime - SinkTime) * 0.16f)) * NPC.velocity.Length() * 0.5f;
            Renderer.DrawScale = new Vector2(1f - wobble * 0.07f, 1f + wobble * 0.07f);
            Renderer.DrawOffset.Y -= (Renderer.DrawScale.Y - 1f) * 20f;
        }

        if (Time >= SinkTime + ConfirmationTime)
        {
            NPC.velocity = Vector2.Zero;

            EndAttack();
            return;
        }
    }

    public bool FlyWheelCondition() => Phase > 0 && Phase < 4;

    public void Attack_FlyWheels()
    {
        EndAttack();
    }

    public void Attack_GapRings()
    {
        const int StartUpTime = 15;
        int WaveCount = 3;
        const int ShootTime = 64;
        int TotalTime = StartUpTime + WaveCount * ShootTime + 140;

        NPCAimedTarget target = NPC.GetTargetData();

        if (Time == 0)
        {
            NPC.FaceTarget();
        }

        if (!target.Invalid)
            TargetPosition = target.Center;

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
                    float curl = Main.rand.NextFloat(-0.5f, 0.5f);
                    int count = (int)(30 + 4 * curl);
                    float randomOffset = Main.rand.NextFloat(-0.2f, 0.2f);
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 direction = new Vector2(1.5f + curl * 0.2f, 0).RotatedBy((float)(i + 1) / (count + 2) * MathHelper.TwoPi);
                        direction = direction.RotatedBy(NPC.AngleTo(TargetPosition) + randomOffset);
                        Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, direction, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                        glowTear.ai[0] = GlowTearType;
                        glowTear.ai[1] = curl * 0.01f * eyeSide;
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