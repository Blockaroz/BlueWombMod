using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static BlueWombMod.Assets;

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
        // public float Rotation = 0f;
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

    public float FaceRotation { get; set; }

    public ref Vector2 DrawScale => ref Body.Scale;
    public ref Vector2 DrawOffset => ref Body.Offset;

    public void Reset()
    {
        HideFace = false;
        InGround = false;
        HideMound = false;

        Body = new Transformation();
        Face = new Transformation();
        FaceRotation = 0f;

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
        if (blinkLeftTime >= 0)
        {
            blinkLeftTime--;

            float blinkCurve = Utils.GetLerpValue(0f, 0.5f, BlinkLeftProgress, true) * Utils.GetLerpValue(1f, 0.6f, BlinkLeftProgress, true);

            EyeLeft.Offset.Y += 12f * blinkCurve;
            EyeLeft.Scale.Y *= 1f - blinkCurve;

            if (blinkCurve > 0.5f)
            {
                EyeLeft.Scale.Y += 0.5f;
                EyeStateLeft = EyeState.Closed;
            }
        }

        if (blinkRightTime >= 0)
        {
            blinkRightTime--;

            float blinkCurve = Utils.GetLerpValue(0f, 0.5f, BlinkRightProgress, true) * Utils.GetLerpValue(1f, 0.6f, BlinkRightProgress, true);

            EyeRight.Offset.Y += 12f * blinkCurve;
            EyeRight.Scale.Y *= 1f - blinkCurve;

            if (blinkCurve > 0.5f)
            {
                EyeRight.Scale.Y += 0.5f;
                EyeStateRight = EyeState.Closed;
            }
        }
    }

    public const int BLINK_TIME = 30;

    private int blinkLeftTime;
    public float BlinkLeftProgress => blinkLeftTime > -1 ? Utils.GetLerpValue(BLINK_TIME, 0, blinkLeftTime, true) : 0f;

    private int blinkRightTime;
    public float BlinkRightProgress => blinkRightTime > -1 ? Utils.GetLerpValue(BLINK_TIME, 0, blinkRightTime, true) : 0f;

    public void Blink()
    {
        blinkLeftTime = BLINK_TIME;
        blinkRightTime = BLINK_TIME;
    }
    public void BlinkLeft()
    {
        blinkLeftTime = BLINK_TIME;
    }
    public void BlinkRight()
    {
        blinkRightTime = BLINK_TIME;
    }

    public static Asset<Texture2D> Texture => TextureAssets.Npc[ModContent.NPCType<Hush>()];
    public static LazyAsset<Texture2D> EyesTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushEyes");
    public static LazyAsset<Texture2D> MouthTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushMouth");

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        var scale = NPC.scale * DrawScale;

        if (!HideMound)
        {
            var texture = Texture.Value;
            Rectangle frame = texture.Frame();
            spriteBatch.Draw(texture, NPC.Center + DrawOffset - screenPos, frame, Color.White, NPC.rotation, frame.Size() / 2, scale, 0, 0);
        }

        if (HideFace)
            return;

        var faceScale = scale * Face.Scale;
        Vector2 faceCenter = NPC.Center + Face.Offset;

        var eyeTexture = EyesTexture.Value;

        int eyeLeftFrameNum = EyeStateLeft == EyeState.Glowing ? 0 : (int)EyeStateLeft;
        int eyeRightFrameNum = EyeStateRight == EyeState.Glowing ? 0 : (int)EyeStateRight;

        Rectangle eyeLeftFrame = eyeTexture.Frame(2, 4, 0, eyeLeftFrameNum);
        Rectangle eyeRightFrame = eyeTexture.Frame(2, 4, 1, eyeRightFrameNum);

        Vector2 eyeLeftOff = (EyeLeft.Offset + new Vector2(-74, -10)).RotatedBy(NPC.rotation + FaceRotation) * faceScale;
        Vector2 eyeRightOff = (EyeRight.Offset + new Vector2(74, -10)).RotatedBy(NPC.rotation + FaceRotation) * faceScale;

        spriteBatch.Draw(eyeTexture, faceCenter + eyeLeftOff + DrawOffset - screenPos, eyeLeftFrame, Color.White, NPC.rotation, eyeLeftFrame.Size() / 2, faceScale * EyeLeft.Scale, 0, 0);
        spriteBatch.Draw(eyeTexture, faceCenter + eyeRightOff + DrawOffset - screenPos, eyeRightFrame, Color.White, NPC.rotation, eyeRightFrame.Size() / 2, faceScale * EyeRight.Scale, 0, 0);

        if (EyeStateLeft == EyeState.Glowing)
        {
            Rectangle eyeLeftGlowFrame = eyeTexture.Frame(2, 4, 0, 0);
            spriteBatch.Draw(eyeTexture, faceCenter + eyeLeftOff + DrawOffset - screenPos, eyeLeftGlowFrame, Color.White, NPC.rotation, eyeLeftGlowFrame.Size() / 2, faceScale * EyeLeft.Scale, 0, 0);
        }
        if (EyeStateRight == EyeState.Glowing)
        {
            Rectangle eyeRightGlowFrame = eyeTexture.Frame(2, 4, 0, 0);
            spriteBatch.Draw(eyeTexture, faceCenter + eyeRightOff + DrawOffset - screenPos, eyeRightGlowFrame, Color.White, NPC.rotation, eyeRightGlowFrame.Size() / 2, faceScale * EyeRight.Scale, 0, 0);
        }

        var mouthTexture = MouthTexture.Value;
        var mouthFrame = mouthTexture.Frame();

        Vector2 mouthOff = (Mouth.Offset + new Vector2(0, 56)).RotatedBy(NPC.rotation + FaceRotation) * faceScale;
        spriteBatch.Draw(mouthTexture, faceCenter + mouthOff + DrawOffset - screenPos, mouthFrame, Color.White, NPC.rotation, mouthFrame.Size() / 2, faceScale * Mouth.Scale, 0, 0);
    }
}