using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace BlueWombMod.Common.Graphics.Camera;

public class ContinuousShakeModifier(
    Vector2 center,
    Vector2 bias,
    float strength,
    int maxTime,
    int frequency = 4,
    string uniqueID = "",
    float fallOff = 10000) : ICameraModifier
{
    private int time = maxTime;
    private Vector2 offset;
    private Vector2 totalOffset;

    public string UniqueIdentity => uniqueID;

    public bool Finished => time < 0;

    public void Update(ref CameraInfo cameraPosition)
    {
        if (!Main.gamePaused)
        {
            offset *= 0.7f;
            if (time % frequency == 0)
                offset = -offset * 0.5f + bias + Main.rand.NextVector2CircularEdge(1, 1) * strength;

            totalOffset *= 0.6f;
            totalOffset += offset;
            time--;
        }

        float fallOffValue = Utils.GetLerpValue(fallOff * 1.2f, fallOff * 0.9f, cameraPosition.OriginalCameraCenter.Distance(center), true);
        float strengthFade = Utils.GetLerpValue(0, maxTime / 2f, time, true);
        cameraPosition.CameraPosition += totalOffset * strengthFade * fallOffValue;
    }
}