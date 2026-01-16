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
using static BlueWombMod.Content.BlueWomb.HushBoss.LittleHush;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed partial class Hush : ModNPC
{
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
        const int StartUpTime = 22;
        const int SalvoTime = 17;
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
                    TargetPosition = Vector2.Lerp(TargetPosition, target.Center, 0.2f);

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
                        TargetPosition = Vector2.Lerp(TargetPosition, target.Center, 0.2f);
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

    public void Attack_HomingVolleys()
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

        NPC.takenDamageMultiplier = 0.2f;

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
        const int FlyCount = 13;
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
                    int orbitCount = 0;
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (npc.ModNPC is HushFly hushFly)
                        {
                            if (hushFly.LeaderIndex == NPC.whoAmI)
                                orbitCount++;
                        }
                    }

                    int flyNumber = (int)MathF.Floor((Time - AhTime) / ChooTime);

                    if (flyNumber == 0)
                    {
                        NPC flyLeader = NPC.NewNPCDirect(NPC.GetSource_FromThis(), nosePosition, ModContent.NPCType<HushFlyLeader>());
                        flyLeader.velocity = new Vector2(0, Main.rand.NextFloat(3f, 7f)).RotatedByRandom(1f);

                        FlyLeaderIndex = flyLeader.whoAmI;
                    }

                    NPC fly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), nosePosition, ModContent.NPCType<HushFly>());
                    fly.velocity = new Vector2(0, Main.rand.NextFloat(3f, 7f)).RotatedByRandom(1f);
                    fly.ai[0] = FlyLeaderIndex;

                    if (flyNumber % 2 == 1 && orbitCount < 20)
                    {
                        NPC orbitFly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), nosePosition, ModContent.NPCType<HushFly>());
                        orbitFly.velocity = new Vector2(0, Main.rand.NextFloat(5f, 15f)).RotatedByRandom(1f);
                        orbitFly.ai[0] = NPC.whoAmI;
                        orbitFly.ai[1] = (int)MathF.Floor(flyNumber / 2f);
                        orbitFly.ai[2] = Time - AhTime - 30;
                    }
                }
            }
        }

        Renderer.DrawOffset.Y -= (Renderer.DrawScale.Y - 1f) * 50;

        if (Time >= TotalTime + 90)
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
                        glowTear.ai[1] = curl * 0.015f * eyeSide;
                        glowTear.localAI[0] = GlowTearType;
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