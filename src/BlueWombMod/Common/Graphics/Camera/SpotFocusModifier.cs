using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace BlueWombMod.Common.Graphics.Camera;

public sealed class SpotFocusModifier(Vector2 position, int moveTime, int holdTime, float fallOff = 8000f, string uniqueID = "") : ICameraModifier
{
    private int time;

    private int totalTime = moveTime * 2 + holdTime;

    public string UniqueIdentity => uniqueID;

    public bool Finished => time > totalTime;

    public void Update(ref CameraInfo cameraPosition)
    {
        if (!Main.gamePaused)
            time++;

        float lerpValue = Utils.GetLerpValue(0, moveTime, time, true) * Utils.GetLerpValue(moveTime * 2 + holdTime, moveTime + holdTime, time, true);

        float fallOffValue = Utils.GetLerpValue(fallOff * 1.02f, fallOff * 0.98f, cameraPosition.OriginalCameraCenter.Distance(position), true);
        Vector2 newPosition = Vector2.Lerp(cameraPosition.OriginalCameraPosition, position - Main.ScreenSize.ToVector2() / 2f, MathHelper.SmoothStep(0, 1, lerpValue) * fallOffValue);
        cameraPosition.CameraPosition += newPosition - cameraPosition.OriginalCameraPosition;
    }
}
