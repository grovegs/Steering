using System.Numerics;

namespace GroveGames.Steering;

public sealed class SeekBehaviour : IContextBehaviour
{
    private Vector2 _direction;
    private float _strength;
    private Vector2 _point;
    private bool _hasPoint;

    public void SetDirection(Vector2 direction, float strength)
    {
        _direction = direction;
        _strength = strength;
        _hasPoint = false;
    }

    public void SetPoint(Vector2 point)
    {
        _point = point;
        _hasPoint = true;
    }

    public void Write(IContextSteering steering, Vector2 position)
    {
        steering.Interest.Write(_direction, _strength);
    }

    public void WritePoints(ContextSteeringSnapshot snapshot)
    {
        if (_hasPoint)
        {
            snapshot.AddInterestPoint(_point);
        }
    }
}
