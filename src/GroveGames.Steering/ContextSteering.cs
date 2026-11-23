using System.Collections.Generic;
using System.Numerics;

namespace GroveGames.Steering;

public sealed class ContextSteering : IContextSteering
{
    private const float MinimumDirectionSquared = 1e-8f;
    private const float MinimumInterest = 1e-4f;

    private readonly ContextMap _interest;
    private readonly ContextMap _danger;
    private readonly float _dangerTolerance;
    private readonly float _interestHalfLife;
    private readonly float _halfSlotAngle;
    private readonly float _halfSlotTangent;
    private readonly float _slotCurvature;
    private readonly List<ContextSteeringSnapshot> _snapshots;
    private bool _isRecording;

    public ContextMap Interest => _interest;
    public ContextMap Danger => _danger;
    public float MaximumDanger => _danger.GetLowestValue() + _dangerTolerance;
    public float InterestHalfLife => _interestHalfLife;
    public bool IsRecording => _isRecording;
    public IReadOnlyList<ContextSteeringSnapshot> Snapshots => _snapshots;

    public ContextSteering(IContextSteeringSettings settings) : this(GetSlotCount(settings), settings.InterestSpread, settings.DangerTolerance, settings.InterestHalfLife)
    {
    }

    public ContextSteering(int slotCount, float interestSpread, float dangerTolerance, float interestHalfLife)
    {
        _interest = new ContextMap(slotCount, interestSpread);
        _danger = new ContextMap(slotCount, 0f);
        _dangerTolerance = Math.Clamp(dangerTolerance, 0f, 1f);
        _interestHalfLife = Math.Max(0f, interestHalfLife);
        var slotAngle = MathF.PI * 2f / _interest.SlotCount;
        _halfSlotAngle = slotAngle * 0.5f;
        _halfSlotTangent = (float)Math.Tan(_halfSlotAngle);
        _slotCurvature = 1f - (float)Math.Cos(slotAngle);
        _snapshots = new List<ContextSteeringSnapshot>();
    }

    public void Clear()
    {
        _interest.Clear();
        _danger.Clear();
    }

    public Vector2 SelectDirection()
    {
        var maximumDanger = MaximumDanger;
        var interest = MinimumInterest;
        var best = -1;

        for (var i = 0; i < _interest.SlotCount; i++)
        {
            if (_danger.GetValue(i) > maximumDanger || _interest.GetValue(i) <= interest)
            {
                continue;
            }

            interest = _interest.GetValue(i);
            best = i;
        }

        if (best < 0)
        {
            return Vector2.Zero;
        }

        GetPeak(best, maximumDanger, out var peakOffset, out _);
        var offset = Math.Clamp(peakOffset, -_halfSlotAngle, _halfSlotAngle);
        var cosine = (float)Math.Cos(offset);
        var sine = (float)Math.Sin(offset);
        var slot = _interest.GetDirection(best);
        return new Vector2(slot.X * cosine - slot.Y * sine, slot.X * sine + slot.Y * cosine);
    }

    public float GetInterest(Vector2 direction)
    {
        if (direction.LengthSquared() < MinimumDirectionSquared)
        {
            return 0f;
        }

        var normalized = Vector2.Normalize(direction);
        var nearest = _interest.GetNearestSlot(normalized);
        var maximumDanger = MaximumDanger;

        if (_danger.GetValue(nearest) > maximumDanger)
        {
            return 0f;
        }

        GetPeak(nearest, maximumDanger, out var peakOffset, out var amplitude);
        var slot = _interest.GetDirection(nearest);
        var offset = (float)Math.Atan2(slot.X * normalized.Y - slot.Y * normalized.X, Vector2.Dot(slot, normalized));
        var interest = _interest.GetValue(nearest);

        if (amplitude > 0f)
        {
            interest += amplitude * ((float)Math.Cos(offset - peakOffset) - (float)Math.Cos(peakOffset));
        }
        else
        {
            var neighbour = (nearest + (offset < 0f ? _interest.SlotCount - 1 : 1)) % _interest.SlotCount;
            interest = Lerp(interest, GetOpenInterest(neighbour, maximumDanger), Math.Abs(offset) / (_halfSlotAngle * 2f));
        }

        return interest > MinimumInterest ? interest : 0f;
    }

    public void SetRecording(bool isRecording)
    {
        _isRecording = isRecording;
    }

    public ContextSteeringSnapshot GetSnapshot(int agentId)
    {
        while (_snapshots.Count <= agentId)
        {
            _snapshots.Add(new ContextSteeringSnapshot(_interest.SlotCount));
        }

        return _snapshots[agentId];
    }

    private static int GetSlotCount(IContextSteeringSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.SlotCount;
    }

    private void GetPeak(int slot, float maximumDanger, out float offset, out float amplitude)
    {
        var previous = (slot + _interest.SlotCount - 1) % _interest.SlotCount;
        var next = (slot + 1) % _interest.SlotCount;
        var before = GetOpenInterest(previous, maximumDanger);
        var after = GetOpenInterest(next, maximumDanger);
        var curve = 2f * _interest.GetValue(slot) - before - after;
        offset = 0f;
        amplitude = 0f;

        if (curve <= MinimumInterest)
        {
            return;
        }

        offset = (float)Math.Atan((after - before) / curve * _halfSlotTangent);

        if (_danger.GetValue(previous) <= maximumDanger && _danger.GetValue(next) <= maximumDanger)
        {
            amplitude = curve * 0.5f / ((float)Math.Cos(offset) * _slotCurvature);
        }
    }

    private float GetOpenInterest(int slot, float maximumDanger)
    {
        return _danger.GetValue(slot) <= maximumDanger ? _interest.GetValue(slot) : 0f;
    }

    private static float Lerp(float from, float to, float weight)
    {
        return from + (to - from) * Math.Clamp(weight, 0f, 1f);
    }
}
