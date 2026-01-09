using Microsoft.Xna.Framework;
using System;
using Terraria;
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

    public void DoSpawn()
    {
        const int PushTime = 60;
        const int BounceOutTime = 30;
        const int TotalTime = PushTime + BounceOutTime;

        if (Time < PushTime)
        {
            float pushProgress = Utils.GetLerpValue(0, PushTime, Time, true);

            Renderer.HideMound = true;
            Renderer.EyeStateLeft = HushRenderer.EyeState.Closed;
            Renderer.EyeStateRight = HushRenderer.EyeState.Closed;

            Renderer.DrawOffset.X = MathF.Sin(Time * 2f) * 4f * MathF.Sin(pushProgress * MathHelper.Pi);
            Renderer.DrawScale = new Vector2(1f + pushProgress * 0.1f, 1f - MathF.Sqrt(pushProgress) * 0.15f);
        }
        else
        {
            float bounceOut = MathF.Sin(Utils.GetLerpValue(PushTime, TotalTime, Time, true) * 8f) * Utils.GetLerpValue(PushTime + BounceOutTime / 1.85f, PushTime + BounceOutTime / 3f, Time, true);
            Renderer.DrawScale = new Vector2(1f - bounceOut * 0.2f, 1f + bounceOut * 0.3f) * (0.8f + 0.2f * MathF.Cbrt(Utils.GetLerpValue(PushTime, PushTime + BounceOutTime / 2f, Time, true)));
            Renderer.Face.Scale *= 1f + MathF.Sqrt(1f - Utils.GetLerpValue(PushTime, PushTime + BounceOutTime / 3f, Time, true)) * 0.2f;
        }

        Time++;

        if (Time >= TotalTime)
        {
            Time = 0;
        }
    }
}