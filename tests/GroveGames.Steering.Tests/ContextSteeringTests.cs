using System.Numerics;

namespace GroveGames.Steering.Tests;

public sealed class ContextSteeringTests
{
    private const int SlotCount = 16;
    private const float InterestSpread = 1f;
    private const float DangerTolerance = 0.1f;
    private const float Tolerance = 1e-4f;
    private const float InterpolationTolerance = 0.01f;

    private readonly ContextSteering _steering;

    public ContextSteeringTests()
    {
        _steering = new ContextSteering(SlotCount, InterestSpread, DangerTolerance, 0f);
    }

    [Fact]
    public void SelectDirection_InterestOnly_ReturnsTheInterestDirectionAtFullInterest()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);

        var direction = _steering.SelectDirection();
        var interest = _steering.GetInterest(direction);

        Assert.True((direction - Vector2.UnitX).Length() < Tolerance);
        Assert.Equal(1f, interest, Tolerance);
    }

    [Fact]
    public void SelectDirection_InterestBetweenSlots_ReturnsTheExactDirectionAtFullInterest()
    {
        var wanted = new Vector2(MathF.Cos(10f * (MathF.PI / 180f)), MathF.Sin(10f * (MathF.PI / 180f)));
        _steering.Interest.Write(wanted * 3f, 1f);

        var direction = _steering.SelectDirection();
        var interest = _steering.GetInterest(direction);

        Assert.True((direction - wanted).Length() < Tolerance);
        Assert.Equal(1f, interest, Tolerance);
    }

    [Fact]
    public void SelectDirection_TwoInterests_ReturnsTheStrongerInterestDirection()
    {
        var strong = new Vector2(MathF.Cos(10f * (MathF.PI / 180f)), MathF.Sin(10f * (MathF.PI / 180f)));
        var weak = new Vector2(MathF.Cos(100f * (MathF.PI / 180f)), MathF.Sin(100f * (MathF.PI / 180f)));
        _steering.Interest.Write(weak, 0.5f);
        _steering.Interest.Write(strong, 1f);

        var direction = _steering.SelectDirection();

        Assert.True((direction - strong).Length() < Tolerance);
    }

    [Fact]
    public void SelectDirection_NoInterest_ReturnsZero()
    {
        _steering.Danger.Write(Vector2.UnitX, 1f);

        var direction = _steering.SelectDirection();
        var interest = _steering.GetInterest(direction);

        Assert.Equal(Vector2.Zero, direction);
        Assert.Equal(0f, interest);
    }

    [Fact]
    public void SelectDirection_DangerAhead_PicksADirectionAwayFromIt()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);
        _steering.Danger.Write(Vector2.UnitX, 1f);

        var direction = _steering.SelectDirection();
        var interest = _steering.GetInterest(direction);

        Assert.True(Vector2.Dot(direction, Vector2.UnitX) <= DangerTolerance + Tolerance);
        Assert.True(Math.Abs(direction.Y) > 0.99f);
        Assert.Equal((Vector2.Dot(direction, Vector2.UnitX) + 1f) * 0.5f, interest, InterpolationTolerance);
    }

    [Fact]
    public void SelectDirection_DangerOnOneSideOfTheWinningSlot_SteersAwayFromThatSide()
    {
        var wanted = new Vector2(MathF.Cos(12f * (MathF.PI / 180f)), MathF.Sin(12f * (MathF.PI / 180f)));
        var danger = new Vector2(MathF.Cos(-30f * (MathF.PI / 180f)), MathF.Sin(-30f * (MathF.PI / 180f)));
        var slot = new Vector2(MathF.Cos(22.5f * (MathF.PI / 180f)), MathF.Sin(22.5f * (MathF.PI / 180f)));
        _steering.Interest.Write(wanted, 1f);
        _steering.Danger.Write(danger, 0.14f);

        var direction = _steering.SelectDirection();

        Assert.True(slot.X * direction.Y - slot.Y * direction.X > Tolerance);
        Assert.True(Vector2.Dot(direction, slot) > MathF.Cos(MathF.PI / SlotCount) - Tolerance);
    }

    [Fact]
    public void SelectDirection_OnlyTheDirectionBehindIsSafe_ReturnsZero()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);
        _steering.Danger.Write(Vector2.UnitX, 1f);
        _steering.Danger.Write(Vector2.UnitY, 1f);
        _steering.Danger.Write(-Vector2.UnitY, 1f);

        var direction = _steering.SelectDirection();
        var interest = _steering.GetInterest(direction);

        Assert.Equal(Vector2.Zero, direction);
        Assert.Equal(0f, interest);
    }

    [Fact]
    public void SelectDirection_DangerEverywhere_ReturnsTheLeastDangerousDirection()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);
        _steering.Danger.Write(Vector2.UnitX, 1f);
        _steering.Danger.Write(Vector2.UnitY, 0.9f);
        _steering.Danger.Write(-Vector2.UnitY, 0.9f);
        _steering.Danger.Write(-Vector2.UnitX, 0.5f);

        var direction = _steering.SelectDirection();
        var interest = _steering.GetInterest(direction);

        Assert.True(Vector2.Dot(direction, -Vector2.UnitX) > 0.9f);
        Assert.True(interest > 0f);
    }

    [Fact]
    public void SelectDirection_HalfSpreadAndOnlySidewaysSafe_ReturnsWeakInterest()
    {
        var steering = new ContextSteering(SlotCount, 0.5f, DangerTolerance, 0f);
        steering.Interest.Write(Vector2.UnitX, 1f);
        steering.Danger.Write(Vector2.UnitX, 1f);

        var direction = steering.SelectDirection();
        var interest = steering.GetInterest(direction);

        Assert.True(Math.Abs(direction.Y) > 0.99f);
        Assert.Equal((Vector2.Dot(direction, Vector2.UnitX) + 0.5f) / 1.5f, interest, InterpolationTolerance);
    }

    [Fact]
    public void SelectDirection_NoSpreadAndOnlySidewaysSafe_ReturnsZero()
    {
        var steering = new ContextSteering(SlotCount, 0f, DangerTolerance, 0f);
        steering.Interest.Write(Vector2.UnitX, 1f);
        steering.Danger.Write(Vector2.UnitX, 1f);

        var direction = steering.SelectDirection();
        var interest = steering.GetInterest(direction);

        Assert.Equal(Vector2.Zero, direction);
        Assert.Equal(0f, interest);
    }

    [Fact]
    public void SelectDirection_NoSpreadAndNoDanger_ReturnsTheInterestDirectionAtFullInterest()
    {
        var steering = new ContextSteering(SlotCount, 0f, DangerTolerance, 0f);
        steering.Interest.Write(Vector2.UnitY, 1f);

        var direction = steering.SelectDirection();
        var interest = steering.GetInterest(direction);

        Assert.True((direction - Vector2.UnitY).Length() < Tolerance);
        Assert.Equal(1f, interest, Tolerance);
    }

    [Fact]
    public void GetInterest_DirectionBesideTheInterest_ReturnsItsShareOfTheInterest()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);

        var interest = _steering.GetInterest(Vector2.UnitY);

        Assert.Equal(0.5f, interest, Tolerance);
    }

    [Fact]
    public void GetInterest_DirectionBetweenSlots_FollowsTheInterestBetweenThem()
    {
        var direction = new Vector2(MathF.Cos(40f * (MathF.PI / 180f)), MathF.Sin(40f * (MathF.PI / 180f)));
        _steering.Interest.Write(Vector2.UnitX, 1f);

        var interest = _steering.GetInterest(direction);

        Assert.Equal((MathF.Cos(40f * (MathF.PI / 180f)) + 1f) * 0.5f, interest, Tolerance);
    }

    [Fact]
    public void GetInterest_DirectionInDanger_ReturnsZero()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);
        _steering.Danger.Write(Vector2.UnitX, 1f);

        var interest = _steering.GetInterest(Vector2.UnitX);

        Assert.Equal(0f, interest);
    }

    [Fact]
    public void GetInterest_ZeroDirection_ReturnsZero()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);

        var interest = _steering.GetInterest(Vector2.Zero);

        Assert.Equal(0f, interest);
    }

    [Fact]
    public void Clear_RemovesPreviousInterestAndDanger()
    {
        _steering.Interest.Write(Vector2.UnitX, 1f);
        _steering.Danger.Write(Vector2.UnitX, 1f);

        _steering.Clear();
        _steering.Interest.Write(Vector2.UnitY, 1f);
        var direction = _steering.SelectDirection();

        Assert.True((direction - Vector2.UnitY).Length() < Tolerance);
    }

    [Fact]
    public void Constructor_Settings_UsesTheirValues()
    {
        var settings = new ContextSteeringSettings(12, 0.5f, 0.2f, 0.3f);

        var steering = new ContextSteering(settings);

        Assert.Equal(12, steering.Interest.SlotCount);
        Assert.Equal(12, steering.Danger.SlotCount);
        Assert.Equal(0.3f, steering.InterestHalfLife);
        Assert.Equal(0.2f, steering.MaximumDanger);
    }
}
