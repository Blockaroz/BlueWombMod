using BlueWombMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed class HushRenderer(NPC nPC)
{
    public NPC NPC { get; } = nPC;

    public bool HideFace { get; set; }

    public bool HideMound { get; set; }

    public AnimationState MoundState { get; set; }

    public enum AnimationState
    {
        Normal,
        InGround,
        InGroundNoFace,
        GaperTunnel
    }

    public enum MouthAnimationState
    {
        Normal,
        Wide,
        Chewing
    }

    public enum EyeAnimationState
    {
        Open,
        Squint,
        Closed,
        Glowing
    }

    public struct Transformation()
    {
        public Vector2 Offset = Vector2.Zero;
        public Vector2 Scale = Vector2.One;
        public float Rotation = 0f;
    }

    private Transformation _bodyTransform;
    public ref Transformation Body => ref _bodyTransform;

    private Transformation _faceTransform;
    public ref Transformation Face => ref _faceTransform;

    public EyeAnimationState EyeStateLeft { get; set; }
    private Transformation _eyeLeftTransform;
    public ref Transformation EyeLeft => ref _eyeLeftTransform;

    public EyeAnimationState EyeStateRight { get; set; }
    private Transformation _eyeRightTransform;
    public ref Transformation EyeRight => ref _eyeRightTransform;

    public Color EyeGlowColorLeft;
    public Color EyeGlowColorRight;

    public MouthAnimationState MouthState { get; set; }
    private Transformation _mouthTransform;
    public ref Transformation Mouth => ref _mouthTransform;

    public ref Vector2 DrawScale => ref Body.Scale;
    public ref Vector2 DrawOffset => ref Body.Offset;

    public void Reset()
    {
        MoundState = AnimationState.Normal;
        HideFace = false;
        HideMound = false;

        Body = new Transformation();
        Face = new Transformation();

        EyeStateLeft = EyeAnimationState.Open;
        EyeLeft = new Transformation();

        EyeStateRight = EyeAnimationState.Open;
        EyeRight = new Transformation();

        MouthState = MouthAnimationState.Normal;
        Mouth = new Transformation();

        EyeGlowColorLeft = Color.Transparent;
        EyeGlowColorRight = Color.Transparent;
    }

    public void Update()
    {
        AnimateEyes();
        AnimateMound();
    }

    public void UpdateForDummy()
    {
        float wobble = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly * 10));
        DrawScale = new Vector2(1f - wobble * 0.02f, 1f + wobble * 0.02f);
    }

    private void AnimateEyes()
    {
        if (blinkLeftTime > 0)
        {
            blinkLeftTime--;

            float blinkCurve = Utils.GetLerpValue(0f, 0.45f, BlinkLeftProgress, true) * Utils.GetLerpValue(1f, 0.6f, BlinkLeftProgress, true);

            EyeLeft.Offset.Y += 16f * blinkCurve;
            EyeLeft.Scale.Y *= 1f - blinkCurve * 0.9f;

            if (blinkCurve > 0.4f)
            {
                EyeLeft.Scale.Y += 0.5f;
                EyeStateLeft = EyeAnimationState.Closed;
            }
        }
        else
            blinkLeftTime = -1;

        if (blinkRightTime > 0)
        {
            blinkRightTime--;

            float blinkCurve = Utils.GetLerpValue(0f, 0.45f, BlinkRightProgress, true) * Utils.GetLerpValue(1f, 0.6f, BlinkRightProgress, true);

            EyeRight.Offset.Y += 16f * blinkCurve;
            EyeRight.Scale.Y *= 1f - blinkCurve * 0.9f;

            if (blinkCurve > 0.4f)
            {
                EyeRight.Scale.Y += 0.5f;
                EyeStateRight = EyeAnimationState.Closed;
            }
        }
        else
            blinkRightTime = -1;
    }

    public const int BLINK_TIME = 20;

    private int blinkLeftTime;
    public bool BlinkingLeft => blinkLeftTime > -1;
    public float BlinkLeftProgress => BlinkingLeft ? Utils.GetLerpValue(BLINK_TIME, 0, blinkLeftTime, true) : 0f;

    private int blinkRightTime;
    public bool BlinkingRight => blinkRightTime > -1;
    public float BlinkRightProgress => BlinkingRight ? Utils.GetLerpValue(BLINK_TIME, 0, blinkRightTime, true) : 0f;

    public void Blink()
    {
        BlinkLeft();
        BlinkRight();
    }

    public void BlinkLeft()
    {
        if (blinkLeftTime == -1 && EyeStateLeft != EyeAnimationState.Closed)
            blinkLeftTime = BLINK_TIME;
        else
            blinkLeftTime = Math.Max(blinkLeftTime, BLINK_TIME / 2);
    }

    public void BlinkRight()
    {
        if (blinkRightTime == -1 && EyeStateRight != EyeAnimationState.Closed)
            blinkRightTime = BLINK_TIME;
        else
            blinkRightTime = Math.Max(blinkRightTime, BLINK_TIME / 2);
    }

    public void GlowLeft(Color? color = null)
    {
        if (color is Color glowColor)
            EyeGlowColorLeft = glowColor;

        if (EyeStateLeft == EyeAnimationState.Open)
            EyeStateLeft = EyeAnimationState.Glowing;
    }

    public void GlowRight(Color? color = null)
    {
        if (color is Color glowColor)
            EyeGlowColorRight = glowColor;

        if (EyeStateRight == EyeAnimationState.Open)
            EyeStateRight = EyeAnimationState.Glowing;
    }

    private int sinkTime;

    public void SinkDown(bool face = true)
    {
        MoundState = face ? AnimationState.InGround : AnimationState.InGroundNoFace;
    }

    private void AnimateMound()
    {
        const int InGroundTime = 18;

        if (MoundState is AnimationState.InGround or AnimationState.InGroundNoFace)
        {
            if (sinkTime < InGroundTime)
                sinkTime++;

            float bounceIntoFloor = MathF.Sin(Utils.GetLerpValue(0, InGroundTime / 2, sinkTime, true) * 2.5f + (MathHelper.Pi - 2.5f));
            DrawScale = new Vector2(1f + 0.2f * bounceIntoFloor, 1f - bounceIntoFloor * 0.1f);
            DrawOffset.Y = (DrawScale.Y - 1f) * -80f;
        }
        else if (sinkTime > 0)
        {
            sinkTime--;

            float bounceOutOfFloor = MathF.Sqrt(Math.Abs(MathF.Sin(Utils.GetLerpValue(InGroundTime, 0, sinkTime, true) * 3f)));
            DrawScale = new Vector2(1f - 0.25f * bounceOutOfFloor, 1f + bounceOutOfFloor * 0.2f);
            DrawOffset.Y = bounceOutOfFloor * -24f;
            Face.Offset.Y += MathF.Sqrt(Utils.GetLerpValue(0, InGroundTime, sinkTime, true)) * -40f;
        }
    }

    public void SetIndependentScales(Vector2 scale)
    {
        EyeLeft.Scale *= scale;
        EyeRight.Scale *= scale;
        Mouth.Scale *= scale;
    }

    public static Asset<Texture2D> Texture => TextureAssets.Npc[ModContent.NPCType<Hush>()];
    public static LazyAsset<Texture2D> EyesTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushEyes");
    public static LazyAsset<Texture2D> MouthTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushMouth");

    public Vector3 GetColor(Vector2 position)
    {
        if (NPC.IsABestiaryIconDummy)
            return Vector3.One;

        // Dust.QuickDust(position, Color.Red);
        return Lighting.GetColor(position.ToTileCoordinates()).ToVector3();
    }

    private void ApplyLight(Vector2 center, float width, float height, float rotation, Vector2 scale)
    {
        Effect lightEffect = Assets.Effects.LightingShader.Value;

        const int lightSize = 8;
        Vector3[] lights = new Vector3[lightSize * lightSize];
        for (int j = 0; j < lightSize; j++)
        {
            for (int i = 0; i < lightSize; i++)
            {
                Vector2 position = center + (new Vector2(width * (i + 0.5f - lightSize / 2f) / lightSize, height * (j + 0.5f - lightSize / 2f) / lightSize) * scale).RotatedBy(rotation);
                lights[i + j * lightSize] = GetColor(position);
            }
        }

        lightEffect.Parameters["uLights"]?.SetValue(lights);

        lightEffect.CurrentTechnique.Passes[0].Apply();
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        var rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
        var scissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
        spriteBatch.End(out var ss);

        var center = Hush.DrawTarget.Size() / 4f;
        var faceScale = Face.Scale * new Vector2(1f - Math.Abs(MathF.Sin(Face.Rotation)) * 0.15f, 1f + Math.Abs(MathF.Sin(Face.Rotation)) * 0.12f);
        var faceRotation = Face.Rotation;

        float faceDist = Utils.GetLerpValue(20, 120, Face.Offset.Length(), true);

        Vector2 mouthPos = center + Face.Offset * 0.75f * faceScale + ((Mouth.Offset + new Vector2(0, 48)) * faceScale).RotatedBy(faceRotation);
        var mouthScale = faceScale * Mouth.Scale * new Vector2(1f, 1f - faceDist);

        Vector2 eyeLeftPos = center + Face.Offset * faceScale + ((EyeLeft.Offset + new Vector2(-74, -16)) * faceScale).RotatedBy(faceRotation);
        var eyeLeftScale = faceScale * EyeLeft.Scale;
        var eyeLeftRot = EyeLeft.Rotation + faceDist * 0.5f;

        Vector2 eyeRightPos = center + Face.Offset * faceScale + ((EyeRight.Offset + new Vector2(74, -16)) * faceScale).RotatedBy(faceRotation);
        var eyeRightScale = faceScale * EyeRight.Scale;
        var eyeRightRot = EyeRight.Rotation - faceDist * 0.5f;

        var texture = Texture.Value;
        var mouthTexture = MouthTexture.Value;
        var eyeTexture = EyesTexture.Value;

        using (new RenderTargetScope(Hush.DrawTarget, clear: true))
        {
            spriteBatch.Begin(ss with { SortMode = SpriteSortMode.Immediate, SamplerState = SamplerState.AnisotropicClamp, Rasterizer = RasterizerState.CullNone, TransformMatrix = Matrix.CreateScale(2f) });

            if (!HideMound)
            {
                Rectangle frame = texture.Frame(1, 4, 0, (int)MoundState);

                spriteBatch.Draw(texture, center, frame, Color.White, 0, frame.Size() / 2, 1f, 0, 0);
            }

            if (!HideFace && MoundState == AnimationState.Normal)
            {
                var mouthFrame = mouthTexture.Frame(1, 3, 0, (int)MouthState);
                spriteBatch.Draw(mouthTexture, mouthPos, mouthFrame, Color.White, faceRotation + Mouth.Rotation, mouthFrame.Size() / 2, mouthScale, 0, 0);

                int eyeLeftFrameNum = EyeStateLeft == EyeAnimationState.Glowing ? 0 : (int)EyeStateLeft;
                int eyeRightFrameNum = EyeStateRight == EyeAnimationState.Glowing ? 0 : (int)EyeStateRight;

                Rectangle eyeLeftFrame = eyeTexture.Frame(2, 4, 0, eyeLeftFrameNum);
                Rectangle eyeRightFrame = eyeTexture.Frame(2, 4, 1, eyeRightFrameNum);

                spriteBatch.Draw(eyeTexture, eyeLeftPos, eyeLeftFrame, Color.White, faceRotation + eyeLeftRot, eyeLeftFrame.Size() / 2, eyeLeftScale, 0, 0);
                spriteBatch.Draw(eyeTexture, eyeRightPos, eyeRightFrame, Color.White, faceRotation + eyeRightRot, eyeRightFrame.Size() / 2, eyeRightScale, 0, 0);
            }

            spriteBatch.End();
        }

        spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;
        spriteBatch.GraphicsDevice.ScissorRectangle = scissorRect;

        spriteBatch.Begin(ss with { SortMode = SpriteSortMode.Immediate });

        Color baseColor = Color.White;

        if (HideMound)
        {
            baseColor = new Color(225, 225, 225) * 0.85f;
        }

        var scale = NPC.scale * DrawScale;

        ApplyLight(NPC.Center + DrawOffset, Hush.DrawTarget.Width, Hush.DrawTarget.Height, NPC.rotation, scale * 0.562f);

        spriteBatch.Draw(Hush.DrawTarget, NPC.Center + DrawOffset - screenPos, Hush.DrawTarget.Frame(), baseColor * NPC.Opacity, NPC.rotation, Hush.DrawTarget.Size() / 2, scale * 0.5f, 0, 0);

        if (!HideFace && MoundState == AnimationState.Normal)
        {
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            faceScale *= scale;
            faceRotation += NPC.rotation;

            eyeLeftPos = NPC.Center + DrawOffset + (eyeLeftPos - center) * faceScale - screenPos;
            eyeRightPos = NPC.Center + DrawOffset + (eyeRightPos - center) * faceScale - screenPos;

            if (EyeStateLeft == EyeAnimationState.Glowing)
            {
                Rectangle eyeLeftGlowFrame = eyeTexture.Frame(2, 4, 0, 3);
                spriteBatch.Draw(eyeTexture, eyeLeftPos, eyeLeftGlowFrame, EyeGlowColorLeft * NPC.Opacity, faceRotation + eyeLeftRot, eyeLeftGlowFrame.Size() / 2, faceScale * EyeLeft.Scale, 0, 0);
                spriteBatch.Draw(eyeTexture, eyeLeftPos, eyeLeftGlowFrame, EyeGlowColorLeft with { A = 0 } * 2f * NPC.Opacity, faceRotation + eyeRightRot, eyeLeftGlowFrame.Size() / 2, faceScale * EyeLeft.Scale, 0, 0);
            }
            if (EyeStateRight == EyeAnimationState.Glowing)
            {
                Rectangle eyeRightGlowFrame = eyeTexture.Frame(2, 4, 1, 3);
                spriteBatch.Draw(eyeTexture, eyeRightPos, eyeRightGlowFrame, EyeGlowColorRight * NPC.Opacity, faceRotation + EyeRight.Rotation, eyeRightGlowFrame.Size() / 2, faceScale * EyeRight.Scale, 0, 0);
                spriteBatch.Draw(eyeTexture, eyeRightPos, eyeRightGlowFrame, EyeGlowColorRight with { A = 0 } * 2f * NPC.Opacity, faceRotation + EyeRight.Rotation, eyeRightGlowFrame.Size() / 2, faceScale * EyeRight.Scale, 0, 0);
            }
        }

        spriteBatch.End();
        spriteBatch.Begin(ss);
    }
}