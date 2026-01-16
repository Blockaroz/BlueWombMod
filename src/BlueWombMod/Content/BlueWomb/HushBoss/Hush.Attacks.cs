using BlueWombMod.Common.Graphics;
using BlueWombMod.Common.Graphics.Camera;
using BlueWombMod.Content.BlueWomb.HushBoss.Minions;
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

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed partial class Hush : ModNPC
{
    public static readonly NPCUtils.SearchFilter<Player> TargetOnlyPlayersInWomb = player => player.ZoneBlueWomb;

    public bool CheckForTarget()
    {
        var results = NPCUtils.SearchForTarget(NPC, NPCUtils.TargetSearchFlag.Players, Hush.TargetOnlyPlayersInWomb);

        if (results.FoundTarget)
        {
            NPC.target = results.NearestTargetIndex;
            NPC.targetRect = results.NearestTargetHitbox;
        }

        if (NPC.GetTargetData().Invalid)
        {
            State = (int)BossState.Despawning;
            Time = 0;
            MiscTime = 0;

            return false;
        }

        return true;
    }

    public void FaceTarget()
    {
        var target = NPC.GetTargetData();
        if (!target.Invalid)
            NPC.direction = NPC.Center.X > target.Center.X ? -1 : 1;
    }

    public void EndAttack()
    {
        Time = 0;
        MiscTime = 0;

        CheckAndPickAttack();
    }

    public int GlowTearType { get; private set => field = Math.Abs(value % 3); }

    public Vector2 TargetPosition { get; set; }

    public void Attack_EyeRingsAlternating()
    {
        const int StartUpTime = 15;
        const int WaveCount = 8;
        const int ShootTime = 29;
        const int TotalTime = StartUpTime + WaveCount * ShootTime + 183;

        if (Time < StartUpTime)
        {
            NPC.FaceTarget();

            Renderer.Blink();

            float progress = Utils.GetLerpValue(0, StartUpTime * 0.8f, Time, true);
            Renderer.DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), MathF.Sqrt(progress));
            Renderer.DrawOffset.X += Main.rand.Next(-8, 8) * progress;

            Renderer.Mouth.Scale.Y *= 1f - MathF.Cbrt(progress) * 0.7f;
        }
        else if (Time < StartUpTime + WaveCount * ShootTime)
        {
            int localTime = (int)(Time - StartUpTime) % ShootTime;
            int alternate = (int)MathF.Floor((float)(Time - StartUpTime) / ShootTime) % 2;
            int eyeSide = (alternate == 0 ? 1 : -1) * NPC.direction;

            Vector2 unsquish = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), Utils.GetLerpValue(StartUpTime + 10, StartUpTime, Time, true));
            float wobble = Utils.PingPongFrom01To010((float)localTime / ShootTime);
            Renderer.DrawScale = Vector2.Lerp(unsquish, new Vector2(0.9f, 1.1f), wobble);
            Renderer.DrawOffset += Main.rand.NextVector2Circular(2, 2) * MathF.Sqrt(wobble);

            Color color = GlowingEyeTear.GetColorFromType(GlowTearType) * Utils.GetLerpValue(ShootTime, ShootTime / 1.5f, localTime, true);

            if (localTime == 0)
            {
                Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Closed;
                Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Closed;

                if (eyeSide < 0)
                    Renderer.BlinkLeft();
                else
                    Renderer.BlinkRight();
            }

            if (localTime > ShootTime - 10)
                Renderer.Blink();

            if (eyeSide < 0)
                Renderer.GlowLeft(color);
            else
                Renderer.GlowRight(color);

            if (localTime == 5 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                GlowTearType = alternate;
                NPC.netUpdate = true;

                Vector2 position = NPC.Center + new Vector2(70 * eyeSide, -10).RotatedBy(Renderer.Face.Rotation + NPC.rotation) * Renderer.DrawScale;
                const int count = 13;

                for (int i = 0; i < count; i++)
                {
                    Vector2 direction = new Vector2(0, -2.5f).RotatedBy((float)i / count * MathHelper.TwoPi + Time * 0.02f * NPC.direction * alternate);
                    Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, direction, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                    glowTear.ai[0] = (int)GlowingEyeTear.Behavior.VelocityLoss;
                    glowTear.ai[1] = 0.008f * eyeSide;
                    glowTear.localAI[0] = GlowTearType;
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

    public void Attack_EyeRingsSpiraling()
    {
        const int StartUpTime = 15;
        const int WaveCount = 5;
        const int ShootTime = 47;
        const int TotalTime = StartUpTime + WaveCount * ShootTime + 150;

        if (Time < StartUpTime)
        {
            NPC.FaceTarget();

            Renderer.Blink();

            float progress = Utils.GetLerpValue(0, StartUpTime * 0.8f, Time, true);
            Renderer.DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), MathF.Sqrt(progress));
            Renderer.DrawOffset.X += Main.rand.Next(-8, 8) * progress;

            Renderer.Mouth.Scale.Y *= 1f - MathF.Cbrt(progress) * 0.7f;
        }
        else if (Time < StartUpTime + WaveCount * ShootTime)
        {
            int localTime = (int)(Time - StartUpTime) % ShootTime;
            int eyeSide = NPC.direction;

            Vector2 unsquish = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), Utils.GetLerpValue(StartUpTime + 10, StartUpTime, Time, true));
            float wobble = Utils.PingPongFrom01To010((float)localTime / ShootTime);
            Renderer.DrawScale = Vector2.Lerp(unsquish, new Vector2(0.9f, 1.1f), wobble);
            Renderer.DrawOffset += Main.rand.NextVector2Circular(2, 2) * MathF.Sqrt(wobble);

            Color color = GlowingEyeTear.GetColorFromType(GlowTearType) * Utils.GetLerpValue(ShootTime, ShootTime / 1.5f, localTime, true);

            if (localTime == 0)
            {
                GlowTearType++;
                Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Closed;
                Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Closed;
            }

            if (localTime > ShootTime - 10)
                Renderer.Blink();

            if (eyeSide < 0)
            {
                Renderer.BlinkRight();
                Renderer.GlowLeft(color);
            }
            else
            {
                Renderer.BlinkLeft();
                Renderer.GlowRight(color);
            }

            const int SprayAmount = 4;
            const int TimePerSpray = 4;

            if (localTime >= 5 && localTime % TimePerSpray == 0 && localTime < 5 + SprayAmount * TimePerSpray && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.netUpdate = true;

                Vector2 position = NPC.Center + new Vector2(70 * eyeSide, -10).RotatedBy(Renderer.Face.Rotation + NPC.rotation) * Renderer.DrawScale;

                float curl = (localTime - 5f) / (SprayAmount * TimePerSpray) % 1f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 direction = new Vector2(0, -4f).RotatedBy((float)i / 4f * MathHelper.TwoPi + curl - Time * 0.01f * eyeSide);
                    Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, direction, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                    glowTear.ai[0] = GlowTearType == 2 ? (int)GlowingEyeTear.Behavior.Forward : (GlowTearType == 1 ? (int)GlowingEyeTear.Behavior.VelocityLoss : (int)GlowingEyeTear.Behavior.VelocityGain);
                    glowTear.ai[1] = -0.001f * eyeSide;
                    glowTear.localAI[0] = GlowTearType;
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

    public void Attack_EyeVolleys()
    {
        const int StartUpTime = 31;
        const int ShootTime = 67;
        const int TotalTime = StartUpTime + ShootTime + 69;

        if (Time < StartUpTime)
        {
            NPC.FaceTarget();

            Renderer.Blink();

            float progress = Utils.GetLerpValue(0, StartUpTime * 0.5f, Time, true);
            Renderer.DrawScale = Vector2.Lerp(Vector2.One, new Vector2(0.9f, 1.1f), MathF.Sqrt(progress));
            Renderer.DrawOffset.X += Main.rand.Next(-8, 8) * progress;

            Renderer.Mouth.Offset.Y += progress * 10f;
            Renderer.Mouth.Scale.X *= 1f - MathF.Cbrt(progress) * 0.3f;
            Renderer.Mouth.Scale.Y *= 1f + MathF.Cbrt(progress) * 0.4f;
        }
        else if (Time < StartUpTime + ShootTime)
        {
            float blastProgress = Utils.GetLerpValue(0, ShootTime / 3f, Time - StartUpTime, true) * Utils.GetLerpValue(ShootTime, ShootTime / 2f, Time - StartUpTime, true);

            Renderer.DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), MathF.Sqrt(blastProgress));
            Renderer.DrawOffset.X += Main.rand.NextFloat(-4f, 4f) * blastProgress;
            int eyeSide = -NPC.direction;

            if (Time == StartUpTime)
            {
                GlowTearType = 2;

                Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Closed;
                Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Closed;
            }

            if (Time > StartUpTime + ShootTime - 10)
                Renderer.Blink();

            Color color = GlowingEyeTear.GetColorFromType(GlowTearType);

            if (eyeSide < 0)
            {
                Renderer.BlinkRight();
                Renderer.GlowLeft(color);
            }
            else
            {
                Renderer.BlinkLeft();
                Renderer.GlowRight(color);
            }

            if (Time % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.netUpdate = true;

                Vector2 eyePosition = NPC.Center + new Vector2(70 * eyeSide, -10).RotatedBy(Renderer.Face.Rotation + NPC.rotation) * Renderer.DrawScale;

                for (int i = 0; i < 3; i++)
                {
                    Vector2 direction = new Vector2(0, 6f).RotatedBy(Time * 0.03f * eyeSide + (float)i / 3f * MathHelper.TwoPi);
                    Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePosition, direction, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                    glowTear.ai[0] = (int)GlowingEyeTear.Behavior.BurstSpeed;
                    glowTear.ai[1] = -0.002f * eyeSide;
                    glowTear.localAI[0] = GlowTearType;
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
        const int StartUpTime = 32;
        const int SalvoTime = 27;
        const int Repeats = 3;
        const int TotalTime = StartUpTime + SalvoTime + 24;
        const int FullAttackTime = TotalTime * Repeats + 60;

        NPCAimedTarget target = NPC.GetTargetData();

        if (Time == 0)
        {
            NPC.FaceTarget();

            if (!target.Invalid)
                TargetPosition = target.Center;
        }

        float wobble = Math.Abs(MathF.Sin(Time * 0.08f)) * Utils.GetLerpValue(StartUpTime / 2f, StartUpTime, Time, true);
        Renderer.DrawScale = new Vector2(1f - wobble * 0.03f, 1f + wobble * 0.03f);

        const float faceOffsetAmount = 50f;

        float localTime = Time % TotalTime;

        if (Time < TotalTime * Repeats)
        {
            if (localTime < StartUpTime)
            {
                if (!target.Invalid)
                    TargetPosition = Vector2.Lerp(TargetPosition, target.Center + target.Velocity * 12f, 0.2f);

                float moveFaceProgress = MathF.Sqrt(Utils.GetLerpValue(0, StartUpTime, Time, true));
                Renderer.Face.Rotation = Utils.AngleLerp(0, NPC.AngleTo(TargetPosition) - MathHelper.PiOver2, moveFaceProgress);
                Renderer.Face.Offset += new Vector2(0, faceOffsetAmount).RotatedBy(Renderer.Face.Rotation) * moveFaceProgress;

                Renderer.Mouth.Scale.Y *= 0.2f + 0.8f * MathF.Sqrt(Utils.GetLerpValue(StartUpTime / 2f, 0, localTime, true));

                float chewProgress = Utils.GetLerpValue(StartUpTime / 1.5f, 0, localTime, true);
                Renderer.EyeLeft.Scale = new Vector2(1f + chewProgress * 0.2f, 1f - chewProgress * 0.15f);
                Renderer.EyeRight.Scale = new Vector2(1f + chewProgress * 0.2f, 1f - chewProgress * 0.15f);

                if (localTime > StartUpTime / 2)
                    Renderer.Blink();
            }
            else
            {
                Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Squint;
                Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Squint;

                float spitProgress = MathF.Sin(Utils.GetLerpValue(0, SalvoTime + 5, localTime - StartUpTime, true) * MathHelper.Pi);

                Renderer.Face.Rotation = NPC.AngleTo(TargetPosition) - MathHelper.PiOver2;
                Renderer.Face.Offset += new Vector2(0, faceOffsetAmount - spitProgress * 13).RotatedBy(Renderer.Face.Rotation);

                Renderer.EyeLeft.Scale.Y += spitProgress * 0.2f;
                Renderer.EyeRight.Scale.Y += spitProgress * 0.2f;
                Renderer.Mouth.Scale.X *= 1f - Utils.GetLerpValue(0f, 0.5f, spitProgress, true) * 0.3f;
                Renderer.Mouth.Scale.Y *= (MathF.Sqrt(Utils.GetLerpValue(0, SalvoTime, localTime - StartUpTime, true)) + spitProgress * 0.67f);

                Renderer.DrawOffset.X += Main.rand.NextFloat(-5f, 5f) * Utils.GetLerpValue(SalvoTime + 5, 0, localTime - StartUpTime, true);

                if (localTime < StartUpTime + SalvoTime)
                {
                    if (localTime == StartUpTime + 1)
                    {
                        SoundEngine.PlaySound(SoundID.Item112 with { Pitch = -0.2f }, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            GlowTearType += Main.rand.Next(1, 3);
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
                else
                {
                    if (!target.Invalid)
                        TargetPosition = Vector2.Lerp(TargetPosition, target.Center + target.Velocity * 2f, 0.2f);
                }
            }
        }
        else
        {
            float faceReturnProgress = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(FullAttackTime, TotalTime * Repeats, Time, true));

            Renderer.Face.Rotation = Utils.AngleLerp(0, NPC.AngleTo(TargetPosition) - MathHelper.PiOver2, faceReturnProgress);
            Renderer.Face.Offset += new Vector2(0, faceOffsetAmount).RotatedBy(Renderer.Face.Rotation) * MathF.Sqrt(faceReturnProgress);
        }

        Renderer.EyeLeft.Offset.Y += (Renderer.EyeLeft.Scale.Y - 1f) * -5f;
        Renderer.EyeRight.Offset.Y += (Renderer.EyeRight.Scale.Y - 1f) * -5f;
        Renderer.Mouth.Offset.Y += (Renderer.Mouth.Scale.Y - 1f) * 7f;

        if (Time >= FullAttackTime + 5)
        {
            EndAttack();
            return;
        }

        Time++;
    }

    public void Attack_Hemorrhage()
    {
        const int ChargeTime = 20;
        const int VolleyTime = 30;
        const int TotalTime = ChargeTime + VolleyTime;
        const int Repeats = 1;

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
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 direction = -Vector2.UnitY.RotatedBy(i / 3f * MathHelper.TwoPi);
                             
                            Vector2 velocity = direction.RotatedByRandom(0.2f) * 2f + Vector2.UnitY * 4f;
                            Projectile bloodSplitter = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), mouthPos, velocity, ModContent.ProjectileType<BloodTear>(), 60, 0f);
                            bloodSplitter.ai[1] = (int)BloodTear.Behavior.BigSplit;
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
        const int SinkTime = 300;
        const int ConfirmationTime = 10;

        NPC.takenDamageMultiplier = 0.1f;

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
                Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 10f, 50, 3, "Hush"));

                BreakRadius();
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

                if (MiscTime % 10 == 0)
                    Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 3f, 15, 2, "Hush"));
            }

            float wobble = Math.Abs(MathF.Sin((MiscTime - SinkTime) * 0.16f)) * NPC.velocity.Length() * 0.5f;
            Renderer.DrawScale = new Vector2(1f - wobble * 0.07f, 1f + wobble * 0.07f);
            Renderer.DrawOffset.Y -= (Renderer.DrawScale.Y - 1f) * 20f;
        }

        if (Time >= SinkTime + ConfirmationTime)
        {
            NPC.Center = HomePosition;
            NPC.velocity = Vector2.Zero;

            Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 10f, 50, 3, "Hush"));

            EndAttack();
            return;
        }
    }

    public bool FlyWheelCondition() => Phase <= 3;

    private int FlyLeaderIndex { get; set; }

    public void Attack_FlyWheels()
    {
        const int AhTime = 58;
        const int ChooTime = 3;
        const int FlyCount = 14;
        const int TotalTime = AhTime + ChooTime * FlyCount;

        if (Time < AhTime)
        {
            float squishDownProgress = Utils.GetLerpValue(0, AhTime / 7f, Time, true);
            Vector2 squishDown = new Vector2(1f + squishDownProgress * 0.3f, 1f - squishDownProgress * 0.5f);
            float stretchUpProgress = MathF.Cbrt(Utils.GetLerpValue(AhTime / 5f, AhTime, Time, true)) + Utils.GetLerpValue(AhTime / 1.3f, AhTime * 1.5f, Time, true);
            Vector2 stretchUp = new Vector2(1f - stretchUpProgress * 0.1f, 1f + stretchUpProgress * 0.1f);
            Renderer.DrawScale = Vector2.Lerp(squishDown, stretchUp, Utils.GetLerpValue(0, AhTime / 6f, Time, true));

            Renderer.Face.Offset.Y -= stretchUpProgress * 15f;

            Renderer.MouthState = HushRenderer.MouthAnimationState.Wide;
            Renderer.Mouth.Scale = new Vector2(0.6f, 1f + MathF.Sin(squishDownProgress * MathHelper.Pi) * 0.3f);
            Renderer.Mouth.Offset.Y += MathF.Sin(squishDownProgress * MathHelper.Pi) * 4f;

            Renderer.EyeLeft.Scale = new Vector2(1f, 1.2f);
            Renderer.EyeRight.Scale = new Vector2(1f, 1.2f);

            if (Time > AhTime / 1.1f)
            {
                Renderer.EyeStateLeft = HushRenderer.EyeAnimationState.Squint;
                Renderer.EyeStateRight = HushRenderer.EyeAnimationState.Squint;
            }
        }
        else if (Time < TotalTime)
        {
            Renderer.Blink();

            float sneezeQuickProgress = MathF.Sqrt(Utils.GetLerpValue(AhTime, AhTime + 12, Time, true));
            Renderer.Mouth.Scale = new Vector2(1.1f + sneezeQuickProgress * 0.3f, 0.3f);

            Renderer.Face.Offset.Y += sneezeQuickProgress * 6f;

            float dropDownAndUp = sneezeQuickProgress * Utils.GetLerpValue(TotalTime, TotalTime - 8, Time, true);
            float wobble = MathF.Sin(MiscTime / 1.5f + dropDownAndUp);
            Vector2 wobbleScale = new Vector2(1f + wobble * 0.1f, 1f - wobble * 0.2f);
            Renderer.DrawScale = Vector2.Lerp(wobbleScale, new Vector2(1f + sneezeQuickProgress * 0.2f - wobble * 0.01f, 1f - MathF.Sqrt(sneezeQuickProgress) * 0.2f + wobble * 0.01f), dropDownAndUp);
            Renderer.DrawOffset.X += Main.rand.NextFloat(-8f, 8f);

            Vector2 nosePosition = NPC.Center + new Vector2(0, 40f);
            if (Time == AhTime)
            {
                for (int i = 0; i < Main.rand.Next(20, 30); i++)
                {
                    Vector2 flyParticleVel = NPC.velocity + new Vector2(0, Main.rand.NextFloat(3f, 10f)).RotatedByRandom(1f);
                    Dust flySmoke = Dust.NewDustPerfect(nosePosition, DustID.Wraith, flyParticleVel, 50, Scale: Main.rand.NextFloat(2f, 4f));
                    flySmoke.noGravity = true;
                }
            }

            if ((Time - AhTime) % ChooTime == 0 && Time < TotalTime)
            {
                Vector2 flyParticleVel = NPC.velocity + new Vector2(0, Main.rand.NextFloat(3f, 10f)).RotatedByRandom(1f);
                var flyParticle = LittleAngryBugParticle.RequestNew(nosePosition, flyParticleVel * 0.6f, Main.rand.Next(80, 200), Main.rand.NextFloat(0.5f, 2f));
                ParticleEngine.Particles.Add(flyParticle);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int flyNumber = (int)MathF.Floor((Time - AhTime) / ChooTime);

                    if (flyNumber == 0)
                    {
                        NPC flyLeader = NPC.NewNPCDirect(NPC.GetSource_FromThis(), nosePosition, ModContent.NPCType<HushFlyLeader>());
                        flyLeader.velocity = new Vector2(0, Main.rand.NextFloat(3f, 7f)).RotatedByRandom(1f);
                        flyLeader.ai[0] = (int)Main.rand.Next([
                            HushFly.HushFlyShape.Circle,
                            HushFly.HushFlyShape.Triangle,
                            HushFly.HushFlyShape.Square,
                            HushFly.HushFlyShape.Pentagon,
                            HushFly.HushFlyShape.Star
                            ]);

                        FlyLeaderIndex = flyLeader.whoAmI;
                    }

                    NPC fly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), nosePosition, ModContent.NPCType<HushFly>());
                    fly.velocity = new Vector2(0, Main.rand.NextFloat(10f, 15f)).RotatedByRandom(1f);
                    fly.ai[0] = FlyLeaderIndex;

                    int orbiterFlyCount = 0;

                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (npc.ModNPC is HushFly hushFly)
                        {
                            if (hushFly.LeaderIndex == NPC.whoAmI)
                            {
                                orbiterFlyCount++;
                            }
                        }
                    }

                    if (flyNumber % 4 == 2 && orbiterFlyCount < 12)
                    {
                        NPC orbitFly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), nosePosition, ModContent.NPCType<HushFly>());
                        orbitFly.velocity = new Vector2(0, Main.rand.NextFloat(1f, 5f)).RotatedByRandom(2f);
                        orbitFly.ai[0] = NPC.whoAmI;
                        orbitFly.ai[1] = orbiterFlyCount;
                        orbitFly.ai[2] = Time - AhTime - 30;
                    }
                }
            }
        }

        Renderer.DrawOffset.Y -= (Renderer.DrawScale.Y - 1f) * 50;

        if (Time >= TotalTime + 134)
        {
            EndAttack();
            return;
        }

        Time++;
    }

    public void Attack_GapRings()
    {
        const int StartUpTime = 15;
        int WaveCount = 3;
        const int ShootTime = 64;
        int TotalTime = StartUpTime + WaveCount * ShootTime + 180;

        NPCAimedTarget target = NPC.GetTargetData();

        if (!target.Invalid)
            TargetPosition = target.Center;

        if (Time < StartUpTime)
        {
            NPC.FaceTarget();

            Renderer.Blink();

            float progress = Utils.GetLerpValue(0, StartUpTime * 0.8f, Time, true);
            Renderer.DrawScale = Vector2.Lerp(Vector2.One, new Vector2(1.1f, 0.9f), MathF.Sqrt(progress));
            Renderer.DrawOffset.X += Main.rand.Next(-8, 8) * progress;

            Renderer.Mouth.Scale.Y *= 1f - MathF.Cbrt(progress) * 0.7f;
        }
        else if (Time < StartUpTime + WaveCount * ShootTime)
        {
            int localTime = (int)(Time - StartUpTime) % ShootTime;
            int eyeSide = NPC.direction;

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
                    int count = (int)(24 + 4 * curl);
                    float randomOffset = Main.rand.NextFloat(-0.2f, 0.2f);
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 direction = new Vector2(1.5f + curl * 0.2f, 0).RotatedBy((float)(i + 1) / (count + 4) * MathHelper.TwoPi);
                        direction = direction.RotatedBy(NPC.AngleTo(TargetPosition) + randomOffset);
                        Projectile glowTear = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), position, direction, ModContent.ProjectileType<GlowingEyeTear>(), 30, 0f);
                        glowTear.ai[1] = curl * 0.015f * eyeSide;
                        glowTear.localAI[0] = GlowTearType;
                    }
                }
            }
        }
        else
            Renderer.Blink();

        if (Time >= TotalTime)
        {
            EndAttack();
            return;
        }

        Time++;
    }

    public void Interphase_Sink()
    {
        const int SinkTime = 300;

        NPC.takenDamageMultiplier = 0.1f;

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
                Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 10f, 50, 3, "Hush"));

                BreakRadius();
            }
        }

        if (Time >= SinkTime)
        {
            NPC.Center = HomePosition;
            NPC.velocity = Vector2.Zero;

            Main.instance.CameraModifiers.Add(new ContinuousShakeModifier(NPC.Center, Vector2.Zero, 10f, 50, 3, "Hush"));

            EndAttack();

            if (Phase == 2)
                State = (int)BossState.GaperTunnel;

            return;
        }
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