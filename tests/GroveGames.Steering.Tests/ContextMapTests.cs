using System.Numerics;

namespace GroveGames.Steering.Tests;

public sealed class ContextMapTests
{
    private const int SlotCount = 16;
    private const int RightSlot = 0;
    private const int UpSlot = 4;
    private const int LeftSlot = 8;
    private const float Tolerance = 1e-4f;

    [Fact]
    public void Blend_HalfWeight_MovesEachSlotHalfwayToThePreviousValue()
    {
        var map = new ContextMap(SlotCount, 0f);
        var previous = new float[SlotCount];
        previous[UpSlot] = 1f;
        map.Write(Vector2.UnitX, 1f);

        map.Blend(previous, 0.5f);

        Assert.Equal(0.5f, map.GetValue(RightSlot), Tolerance);
        Assert.Equal(0.5f, map.GetValue(UpSlot), Tolerance);
    }

    [Fact]
    public void CopyTo_AfterWrite_CopiesEverySlot()
    {
        var map = new ContextMap(SlotCount, 0f);
        var values = new float[SlotCount];
        map.Write(Vector2.UnitY, 0.8f);

        map.CopyTo(values);

        Assert.Equal(0.8f, values[UpSlot], Tolerance);
        Assert.Equal(map.GetValue(LeftSlot), values[LeftSlot]);
    }

    [Fact]
    public void Write_FullSpread_FallsOffToZeroBehind()
    {
        var map = new ContextMap(SlotCount, 1f);

        map.Write(Vector2.UnitX, 1f);

        Assert.Equal(1f, map.GetValue(RightSlot), Tolerance);
        Assert.Equal(0.5f, map.GetValue(UpSlot), Tolerance);
        Assert.Equal(0f, map.GetValue(LeftSlot), Tolerance);
    }

    [Fact]
    public void Write_NoSpread_FallsOffToZeroSideways()
    {
        var map = new ContextMap(SlotCount, 0f);

        map.Write(Vector2.UnitX, 0.8f);

        Assert.Equal(0.8f, map.GetValue(RightSlot), Tolerance);
        Assert.Equal(0f, map.GetValue(UpSlot), Tolerance);
        Assert.Equal(0f, map.GetValue(LeftSlot));
    }

    [Fact]
    public void Write_WeakerValueOverStronger_KeepsTheStronger()
    {
        var map = new ContextMap(SlotCount, 1f);
        map.Write(Vector2.UnitX, 1f);

        map.Write(Vector2.UnitX, 0.4f);

        Assert.Equal(1f, map.GetValue(RightSlot), Tolerance);
    }

    [Fact]
    public void Write_ZeroDirection_IsIgnored()
    {
        var map = new ContextMap(SlotCount, 1f);

        map.Write(Vector2.Zero, 1f);

        Assert.Equal(0f, map.GetValue(RightSlot));
    }

    [Fact]
    public void Clear_AfterWrite_ResetsEverySlot()
    {
        var map = new ContextMap(SlotCount, 1f);
        map.Write(Vector2.UnitX, 1f);

        map.Clear();

        Assert.Equal(0f, map.GetValue(RightSlot));
        Assert.Equal(0f, map.GetValue(UpSlot));
    }

    [Fact]
    public void GetLowestValue_AfterWrite_ReturnsTheSmallestSlot()
    {
        var map = new ContextMap(SlotCount, 0f);
        map.Write(Vector2.UnitX, 1f);
        map.Write(-Vector2.UnitX, 0.5f);

        var lowest = map.GetLowestValue();

        Assert.Equal(0f, lowest, Tolerance);
    }

    [Fact]
    public void GetNearestSlot_DirectionBetweenSlots_ReturnsTheCloserSlot()
    {
        var map = new ContextMap(SlotCount, 1f);
        var direction = new Vector2(MathF.Cos(80f * (MathF.PI / 180f)), MathF.Sin(80f * (MathF.PI / 180f)));

        var slot = map.GetNearestSlot(direction);

        Assert.Equal(UpSlot, slot);
        Assert.True((map.GetDirection(slot) - Vector2.UnitY).Length() < Tolerance);
    }
}
