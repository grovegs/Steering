using System;
using System.Numerics;

namespace GroveGames.Steering;

public sealed class ContextSteeringAgent
{
    private readonly int _id;
    private readonly IContextSteering _steering;
    private readonly IContextBehaviour[] _behaviours;
    private readonly float[] _previousInterest;
    private readonly float[] _candidateInterest;
    private readonly float[] _usedInterest;
    private float _blendWeight;
    private bool _hasPreviousInterest;
    private bool _hasCandidateInterest;
    private bool _hasUsedInterest;

    public ContextSteeringAgent(int id, IContextSteering steering, IContextBehaviour[] behaviours)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(id);
        ArgumentNullException.ThrowIfNull(steering);
        ArgumentNullException.ThrowIfNull(behaviours);

        _id = id;
        _steering = steering;
        _behaviours = behaviours;
        _previousInterest = new float[steering.Interest.SlotCount];
        _candidateInterest = new float[steering.Interest.SlotCount];
        _usedInterest = new float[steering.Interest.SlotCount];
    }

    public Vector2 SelectDirection(Vector2 position)
    {
        _steering.Clear();

        for (var i = 0; i < _behaviours.Length; i++)
        {
            _behaviours[i].Write(_steering, position);
        }

        if (_hasPreviousInterest)
        {
            _steering.Interest.Blend(_previousInterest, _blendWeight);
        }

        _steering.Interest.CopyTo(_candidateInterest);
        _hasCandidateInterest = true;
        var direction = _steering.SelectDirection();

        if (_steering.IsRecording)
        {
            Record(position, direction);
        }

        return direction;
    }

    public void Commit()
    {
        if (!_hasCandidateInterest)
        {
            return;
        }

        Array.Copy(_candidateInterest, _usedInterest, _usedInterest.Length);
        _hasUsedInterest = true;
    }

    public void CompleteFrame(float deltaTime)
    {
        if (_hasUsedInterest)
        {
            Array.Copy(_usedInterest, _previousInterest, _previousInterest.Length);
        }

        _hasPreviousInterest = _hasUsedInterest;
        _hasUsedInterest = false;
        _hasCandidateInterest = false;
        var halfLife = _steering.InterestHalfLife;
        _blendWeight = halfLife > 0f ? MathF.Pow(0.5f, Math.Max(0f, deltaTime) / halfLife) : 0f;
    }

    public void Reset()
    {
        _hasPreviousInterest = false;
        _hasCandidateInterest = false;
        _hasUsedInterest = false;
    }

    public float GetInterest(Vector2 direction)
    {
        return _steering.GetInterest(direction);
    }

    private void Record(Vector2 position, Vector2 direction)
    {
        var snapshot = _steering.GetSnapshot(_id);
        snapshot.Record(_steering, position, direction);

        for (var i = 0; i < _behaviours.Length; i++)
        {
            _behaviours[i].WritePoints(snapshot);
        }
    }
}
