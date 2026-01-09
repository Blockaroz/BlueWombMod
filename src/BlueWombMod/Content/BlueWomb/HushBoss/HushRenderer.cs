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

    public bool InGround { get; set; }

    public bool HideMound { get; set; }

    public enum EyeState
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

    public EyeState EyeStateLeft { get; set; }
    private Transformation _eyeLeftTransform;
    public ref Transformation EyeLeft => ref _eyeLeftTransform;

    public EyeState EyeStateRight { get; set; }
    private Transformation _eyeRightTransform;
    public ref Transformation EyeRight => ref _eyeRightTransform;

    private Transformation _mouthTransform;
    public ref Transformation Mouth => ref _mouthTransform;

    public ref Vector2 DrawScale => ref Body.Scale;
    public ref Vector2 DrawOffset => ref Body.Offset;

    public void Reset()
    {
        HideFace = false;
        InGround = false;
        HideMound = false;

        Body = new Transformation();
        Face = new Transformation();

        EyeStateLeft = EyeState.Open;
        EyeLeft = new Transformation();

        EyeStateRight = EyeState.Open;
        EyeRight = new Transformation();

        Mouth = new Transformation();
    }

    public void Update()
    {
        AnimateEyes();
    }

    public void UpdateForDummy()
    {

    }

    private void AnimateEyes()
    {
        if (blinkLeftTime > 0)
        {
            blinkLeftTime--;

            float blinkCurve = Utils.GetLerpValue(0f, 0.45f, BlinkLeftProgress, true) * Utils.GetLerpValue(1f, 0.6f, BlinkLeftProgress, true);

            EyeLeft.Offset.Y += 12f * blinkCurve;
            EyeLeft.Scale.Y *= 1f - blinkCurve * 0.9f;

            if (blinkCurve > 0.4f)
            {
                EyeLeft.Scale.Y += 0.8f;
                EyeStateLeft = EyeState.Closed;
            }
        }
        else
            blinkLeftTime = -1;

        if (blinkRightTime > 0)
        {
            blinkRightTime--;

            float blinkCurve = Utils.GetLerpValue(0f, 0.45f, BlinkRightProgress, true) * Utils.GetLerpValue(1f, 0.6f, BlinkRightProgress, true);

            EyeRight.Offset.Y += 12f * blinkCurve;
            EyeRight.Scale.Y *= 1f - blinkCurve * 0.9f;

            if (blinkCurve > 0.4f)
            {
                EyeRight.Scale.Y += 0.8f;
                EyeStateRight = EyeState.Closed;
            }
        }
        else
            blinkRightTime = -1;
    }

    public const int BLINK_TIME = 24;

    private int blinkLeftTime;
    public float BlinkLeftProgress => blinkLeftTime > -1 ? Utils.GetLerpValue(BLINK_TIME, 0, blinkLeftTime, true) : 0f;

    private int blinkRightTime;
    public float BlinkRightProgress => blinkRightTime > -1 ? Utils.GetLerpValue(BLINK_TIME, 0, blinkRightTime, true) : 0f;

    public void Blink()
    {
        BlinkLeft();
        BlinkRight();
    }

    public void BlinkLeft()
    {
        if (blinkLeftTime == -1 && EyeStateLeft != EyeState.Closed)
            blinkLeftTime = BLINK_TIME;
        else
            blinkLeftTime = Math.Max(blinkLeftTime, BLINK_TIME / 2);
    }
    public void BlinkRight()
    {
        if (blinkRightTime == -1 && EyeStateRight != EyeState.Closed)
            blinkRightTime = BLINK_TIME;
        else
            blinkRightTime = Math.Max(blinkRightTime, BLINK_TIME / 2);
    }

    public static Asset<Texture2D> Texture => TextureAssets.Npc[ModContent.NPCType<Hush>()];
    public static LazyAsset<Texture2D> EyesTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushEyes");
    public static LazyAsset<Texture2D> MouthTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushMouth");

    public Vector3 GetColor(Vector2 position)
    {
        if (NPC.IsABestiaryIconDummy)
            return Vector3.One;

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
                //Dust.QuickDust(position, Color.Red);
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
        var faceScale = Face.Scale * new Vector2(1f - Math.Abs(MathF.Sin(Face.Rotation)) * 0.2f, 1f + Math.Abs(MathF.Sin(Face.Rotation)) * 0.1f);
        var faceRotation = Face.Rotation;

        Vector2 mouthPos = center + Face.Offset * 0.75f * faceScale + ((Mouth.Offset + new Vector2(0, 50)) * faceScale).RotatedBy(faceRotation);
        var mouthScale = faceScale * Mouth.Scale * new Vector2(1f, 1f - Utils.GetLerpValue(20, 120, Face.Offset.Length(), true));

        Vector2 eyeLeftPos = center + Face.Offset * faceScale + ((EyeLeft.Offset + new Vector2(-74, -16)) * faceScale).RotatedBy(faceRotation);
        var eyeLeftScale = faceScale * EyeLeft.Scale;

        Vector2 eyeRightPos = center + Face.Offset * faceScale + ((EyeRight.Offset + new Vector2(74, -16)) * faceScale).RotatedBy(faceRotation);
        var eyeRightScale = faceScale * EyeRight.Scale;

        var texture = Texture.Value;
        var mouthTexture = MouthTexture.Value;
        var eyeTexture = EyesTexture.Value;

        using (new RenderTargetScope(Hush.DrawTarget, clear: true))
        {
            spriteBatch.Begin(ss with { SortMode = SpriteSortMode.Immediate, Rasterizer = RasterizerState.CullNone, TransformMatrix = Matrix.CreateScale(2f) });

            if (!HideMound)
            {
                Rectangle frame = texture.Frame();

                spriteBatch.Draw(texture, center, frame, Color.White, NPC.rotation, frame.Size() / 2, 1f, 0, 0);
            }

            if (!HideFace)
            {
                var mouthFrame = mouthTexture.Frame();
                spriteBatch.Draw(mouthTexture, mouthPos, mouthFrame, Color.White, faceRotation + Mouth.Rotation, mouthFrame.Size() / 2, mouthScale, 0, 0);

                int eyeLeftFrameNum = EyeStateLeft == EyeState.Glowing ? 0 : (int)EyeStateLeft;
                int eyeRightFrameNum = EyeStateRight == EyeState.Glowing ? 0 : (int)EyeStateRight;

                Rectangle eyeLeftFrame = eyeTexture.Frame(2, 4, 0, eyeLeftFrameNum);
                Rectangle eyeRightFrame = eyeTexture.Frame(2, 4, 1, eyeRightFrameNum);

                spriteBatch.Draw(eyeTexture, eyeLeftPos, eyeLeftFrame, Color.White, faceRotation + EyeLeft.Rotation, eyeLeftFrame.Size() / 2, eyeLeftScale, 0, 0);
                spriteBatch.Draw(eyeTexture, eyeRightPos, eyeRightFrame, Color.White, faceRotation + EyeRight.Rotation, eyeRightFrame.Size() / 2, eyeRightScale, 0, 0);

                spriteBatch.End();
            }
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

        ApplyLight(NPC.Center + DrawOffset, Hush.DrawTarget.Width, Hush.DrawTarget.Height, NPC.rotation, scale * 0.5f);

        spriteBatch.Draw(Hush.DrawTarget, NPC.Center + DrawOffset - screenPos, Hush.DrawTarget.Frame(), baseColor * NPC.Opacity, NPC.rotation, Hush.DrawTarget.Size() / 2, scale * 0.5f, 0, 0);

        faceScale *= scale;
        faceRotation += NPC.rotation;

        eyeLeftPos += NPC.Center + DrawOffset - center - screenPos;
        eyeRightPos += NPC.Center + DrawOffset - center - screenPos;

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        if (EyeStateLeft == EyeState.Glowing)
        {
            Rectangle eyeLeftGlowFrame = eyeTexture.Frame(2, 4, 0, 3);
            spriteBatch.Draw(eyeTexture, eyeLeftPos, eyeLeftGlowFrame, Color.DarkCyan with { A = 200 } * NPC.Opacity, faceRotation + EyeLeft.Rotation, eyeLeftGlowFrame.Size() / 2, faceScale * EyeLeft.Scale, 0, 0);
            spriteBatch.Draw(eyeTexture, eyeLeftPos, eyeLeftGlowFrame, Color.White with { A = 80 } * NPC.Opacity * 0.75f, faceRotation + EyeLeft.Rotation, eyeLeftGlowFrame.Size() / 2, faceScale * EyeLeft.Scale, 0, 0);
        }
        if (EyeStateRight == EyeState.Glowing)
        {
            Rectangle eyeRightGlowFrame = eyeTexture.Frame(2, 4, 1, 3);
            spriteBatch.Draw(eyeTexture, eyeRightPos, eyeRightGlowFrame, Color.Yellow with { A = 200 } * NPC.Opacity, faceRotation + EyeRight.Rotation, eyeRightGlowFrame.Size() / 2, faceScale * EyeRight.Scale, 0, 0);
            spriteBatch.Draw(eyeTexture, eyeRightPos, eyeRightGlowFrame, Color.White with { A = 80 } * NPC.Opacity * 0.75f, faceRotation + EyeRight.Rotation, eyeRightGlowFrame.Size() / 2, faceScale * EyeRight.Scale, 0, 0);
        }

        spriteBatch.End();
        spriteBatch.Begin(ss);
    }
}