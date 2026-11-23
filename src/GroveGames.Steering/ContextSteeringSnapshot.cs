using System.Collections.Generic;
using System.Numerics;

namespace GroveGames.Steering;

public sealed class ContextSteeringSnapshot
{
    private readonly float[] _interest;
    private readonly float[] _danger;
    private readonly List<Vector2> _interestPoints;
    private readonly List<Vector2> _dangerPoints;
    private Vector2 _position;
    private Vector2 _direction;
    private float _maximumDanger;
    private bool _isRecorded;

    public int SlotCount => _interest.Length;
    public Vector2 Position => _position;
    public Vector2 Direction => _direction;
    public bool IsRecorded => _isRecorded;
    public IReadOnlyList<Vector2> InterestPoints => _interestPoints;
    public IReadOnlyList<Vector2> DangerPoints => _dangerPoints;

    public ContextSteeringSnapshot(int slotCount)
    {
        _interest = new float[slotCount];
        _danger = new float[slotCount];
        _interestPoints = new List<Vector2>();
        _dangerPoints = new List<Vector2>();
    }

    public float GetInterest(int slot)
    {
        return _interest[slot];
    }

    public float GetDanger(int slot)
    {
        return _danger[slot];
    }

    public bool IsBlocked(int slot)
    {
        return _danger[slot] > _maximumDanger;
    }

    public void Record(IContextSteering steering, Vector2 position, Vector2 direction)
    {
        _position = position;
        _direction = direction;
        _isRecorded = true;
        _maximumDanger = steering.MaximumDanger;
        _interestPoints.Clear();
        _dangerPoints.Clear();

        for (var i = 0; i < _interest.Length; i++)
        {
            _interest[i] = steering.Interest.GetValue(i);
            _danger[i] = steering.Danger.GetValue(i);
        }
    }

    public void Clear()
    {
        _isRecorded = false;
        _interestPoints.Clear();
        _dangerPoints.Clear();
    }

    public void AddInterestPoint(Vector2 point)
    {
        _interestPoints.Add(point);
    }

    public void AddDangerPoint(Vector2 point)
    {
        _dangerPoints.Add(point);
    }
}
