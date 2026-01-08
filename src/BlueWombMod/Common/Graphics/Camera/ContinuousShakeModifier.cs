using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace BlueWombMod.Common.Graphics.Camera;

public class ContinuousShakeModifier : ICameraModifier
{
    public ContinuousShakeModifier(Vector2 center, Vector2 bias, float strength, int time, int frequency = 4, string uniqueID = "", float fallOff = 10000)
    {
        _center = center;
        _bias = bias;
        _strength = strength;
        _maxTime = time;
        _time = time;
        _frequency = frequency;
        _uniqueID = uniqueID;
        _fallOff = fallOff;
    }

    private float _strength;
    private int _maxTime;
    private int _time;
    private int _frequency;
    private float _fallOff;

    private Vector2 _center;
    private Vector2 _offset;
    private Vector2 _totalOffset;
    private Vector2 _bias;
    private string _uniqueID;

    public string UniqueIdentity => _uniqueID;

    public bool Finished => _time <= 0;

    public void Update(ref CameraInfo cameraPosition)
    {
        if (!Main.gamePaused)
        {
            _offset *= 0.7f;
            if (_time % _frequency == 0)
                _offset = -_offset * 0.5f + _bias + Main.rand.NextVector2CircularEdge(1, 1) * _strength;

            _totalOffset *= 0.6f;
            _totalOffset += _offset;
            _time--;
        }

        float fallOff = Utils.GetLerpValue(_fallOff * 1.5f, _fallOff * 0.9f, cameraPosition.OriginalCameraCenter.Distance(_center), true);
        float strengthFade = Utils.GetLerpValue(0, _maxTime / 2f, _time, true);
        cameraPosition.CameraPosition = cameraPosition.OriginalCameraPosition + _totalOffset * strengthFade * fallOff;
    }
}