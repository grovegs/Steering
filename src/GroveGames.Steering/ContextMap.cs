using System;
using System.Numerics;

namespace GroveGames.Steering;

public sealed class ContextMap
{
    private const float MinimumDirectionSquared = 1e-8f;
    private const int MinimumSlotCount = 4;

    private readonly Vector2[] _slots;
    private readonly float[] _values;
    private readonly float _spread;

    public int SlotCount => _values.Length;

    public ContextMap(int slotCount, float spread)
    {
        var count = Math.Max(MinimumSlotCount, slotCount);
        _slots = new Vector2[count];
        _values = new float[count];
        _spread = Math.Clamp(spread, 0f, 1f);

        for (var i = 0; i < count; i++)
        {
            var angle = i * MathF.PI * 2f / count;
            _slots[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }

    public void Clear()
    {
        Array.Clear(_values, 0, _values.Length);
    }

    public void Write(Vector2 direction, float strength)
    {
        if (strength <= 0f || direction.LengthSquared() < MinimumDirectionSquared)
        {
            return;
        }

        var normalized = Vector2.Normalize(direction);

        for (var i = 0; i < _slots.Length; i++)
        {
            var value = (Vector2.Dot(_slots[i], normalized) + _spread) / (1f + _spread) * strength;

            if (value > _values[i])
            {
                _values[i] = value;
            }
        }
    }

    public void Blend(float[] previous, float weight)
    {
        var count = Math.Min(_values.Length, previous.Length);

        for (var i = 0; i < count; i++)
        {
            _values[i] = Lerp(_values[i], previous[i], weight);
        }
    }

    public void CopyTo(float[] values)
    {
        Array.Copy(_values, values, Math.Min(_values.Length, values.Length));
    }

    public float GetValue(int slot)
    {
        return _values[slot];
    }

    public Vector2 GetDirection(int slot)
    {
        return _slots[slot];
    }

    public float GetLowestValue()
    {
        var lowest = float.MaxValue;

        for (var i = 0; i < _values.Length; i++)
        {
            lowest = Math.Min(lowest, _values[i]);
        }

        return lowest;
    }

    public int GetNearestSlot(Vector2 direction)
    {
        var nearest = 0;

        for (var i = 1; i < _slots.Length; i++)
        {
            if (Vector2.Dot(_slots[i], direction) > Vector2.Dot(_slots[nearest], direction))
            {
                nearest = i;
            }
        }

        return nearest;
    }

    private static float Lerp(float from, float to, float weight)
    {
        return from + (to - from) * Math.Clamp(weight, 0f, 1f);
    }
}
