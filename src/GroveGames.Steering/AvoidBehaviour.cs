using System;
using System.Numerics;

namespace GroveGames.Steering;

public sealed class AvoidBehaviour : IContextBehaviour
{
    private Vector2[] _obstacles;
    private int _count;
    private float _radius;

    public AvoidBehaviour()
    {
        _obstacles = Array.Empty<Vector2>();
        _count = 0;
        _radius = 0f;
    }

    public void SetObstacles(Vector2[] obstacles, int count, float radius)
    {
        _obstacles = obstacles;
        _count = Math.Min(count, obstacles.Length);
        _radius = radius;
    }

    public void Write(IContextSteering steering, Vector2 position)
    {
        for (var i = 0; i < _count; i++)
        {
            var toObstacle = _obstacles[i] - position;
            var distance = toObstacle.Length();

            if (distance < _radius)
            {
                steering.Danger.Write(toObstacle, 1f - distance / _radius);
            }
        }
    }

    public void WritePoints(ContextSteeringSnapshot snapshot)
    {
        for (var i = 0; i < _count; i++)
        {
            if ((_obstacles[i] - snapshot.Position).LengthSquared() < _radius * _radius)
            {
                snapshot.AddDangerPoint(_obstacles[i]);
            }
        }
    }
}
