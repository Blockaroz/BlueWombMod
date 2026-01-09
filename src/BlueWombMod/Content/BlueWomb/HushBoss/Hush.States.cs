using BlueWombMod.Content.BlueWomb.Tiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed partial class Hush : ModNPC
{
    public Vector2 HomePosition => new Point(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates();

    public void SetHome(Vector2 position)
    {
        Point pt = position.ToTileCoordinates();
        NPC.homeTileX = pt.X;
        NPC.homeTileY = pt.Y;
    }

    public void BreakRadius()
    {
        const int radius = 250 / 16;
        for (int j = -radius; j < radius; j++)
        {
            for (int i = -radius; i < radius; i++)
            {
                float dist = MathF.Sqrt(i * i + j * j);
                if (dist < radius)
                {
                    Point tilePos = NPC.Center.ToTileCoordinates() + new Point(i, j);
                    WorldGen.KillTile(tilePos.X, tilePos.Y);
                }
            }
        }
    }

    public void DoSpawn()
    {
        const int PushTime = 95;
        const int BounceOutTime = 35;
        const int TotalTime = PushTime + BounceOutTime;

        NPC.dontTakeDamage = true;

        if (Time == PushTime)
        {
            // BreakRadius();
        }

        if (Time < PushTime)
        {
            float pushProgress = Utils.GetLerpValue(0, PushTime, Time, true);
            float sqrtProgress = MathF.Sqrt(pushProgress);
            Renderer.HideMound = true;

            Renderer.DrawOffset.X = MathF.Sin(Time * 1.75f) * 6f * Utils.GetLerpValue(0.2f, 0.8f, MathF.Sin(pushProgress * 2f), true);
            Renderer.DrawScale = new Vector2(1f + sqrtProgress * 0.1f, 1f - sqrtProgress * 0.1f);
            Renderer.Mouth.Scale = new Vector2(0.9f + sqrtProgress * 0.2f, 1f - sqrtProgress * 0.2f);

            Renderer.EyeStateLeft = HushRenderer.EyeState.Closed;
            Renderer.EyeLeft.Offset.Y -= 7f;
            Renderer.EyeLeft.Rotation = 0.2f;
            Renderer.EyeStateRight = HushRenderer.EyeState.Closed;
            Renderer.EyeRight.Offset.Y -= 7f;
            Renderer.EyeRight.Rotation = -0.2f;
            Renderer.Blink();
        }
        else if (Time <= TotalTime)
        {
            float bounceOut = MathF.Sin(Utils.GetLerpValue(PushTime, TotalTime, Time, true) * 8f) * Utils.GetLerpValue(TotalTime, PushTime + BounceOutTime / 3f, Time, true);
            Renderer.DrawScale = new Vector2(1f - bounceOut * 0.15f, 1f + bounceOut * 0.2f) * (0.4f + 0.6f * MathF.Cbrt(Utils.GetLerpValue(PushTime, PushTime + BounceOutTime / 2f, Time, true)));
            Renderer.Face.Scale *= 1f + MathF.Sqrt(1f - Utils.GetLerpValue(PushTime, PushTime + BounceOutTime / 3f, Time, true)) * 0.25f;
            Renderer.Face.Offset.Y = bounceOut * 12;
            Renderer.Face.Rotation = MathF.Sin(Time * 0.5f) * 0.1f * Utils.GetLerpValue(PushTime + BounceOutTime / 1.5f, PushTime + BounceOutTime / 3f, Time, true);

            if (Time == PushTime)
            {

            }
        }

        NPC.Opacity = Utils.GetLerpValue(2, 8, Time, true);

        Time++;

        if (Time >= TotalTime + 121)
        {
            NPC.dontTakeDamage = false;

            Time = 0;
        }
    }
}