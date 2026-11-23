using System.Numerics;

namespace GroveGames.Steering.Tests;

public sealed class AvoidBehaviourTests
{
    private const int SlotCount = 16;
    private const int RightSlot = 0;
    private const int UpSlot = 4;
    private const float Radius = 2f;
    private const float Tolerance = 1e-4f;

    private readonly ContextSteering _steering;
    private readonly AvoidBehaviour _behaviour;

    public AvoidBehaviourTests()
    {
        _steering = new ContextSteering(SlotCount, 1f, 0.1f, 0f);
        _behaviour = new AvoidBehaviour();
    }

    [Fact]
    public void Write_ObstacleInsideTheRadius_WritesDangerTowardItByCloseness()
    {
        _behaviour.SetObstacles(new[] { new Vector2(0.5f, 0f) }, 1, Radius);

        _behaviour.Write(_steering, Vector2.Zero);

        Assert.Equal(0.75f, _steering.Danger.GetValue(RightSlot), Tolerance);
        Assert.Equal(0f, _steering.Danger.GetValue(UpSlot), Tolerance);
    }

    [Fact]
    public void Write_ObstacleOutsideTheRadius_WritesNothing()
    {
        _behaviour.SetObstacles(new[] { new Vector2(3f, 0f) }, 1, Radius);

        _behaviour.Write(_steering, Vector2.Zero);

        Assert.Equal(0f, _steering.Danger.GetValue(RightSlot));
    }

    [Fact]
    public void Write_CountSmallerThanTheBuffer_IgnoresTheRest()
    {
        _behaviour.SetObstacles(new[] { new Vector2(3f, 0f), new Vector2(0f, 0.5f) }, 1, Radius);

        _behaviour.Write(_steering, Vector2.Zero);

        Assert.Equal(0f, _steering.Danger.GetValue(UpSlot));
    }

    [Fact]
    public void Write_MovedPosition_MeasuresFromThatPosition()
    {
        _behaviour.SetObstacles(new[] { new Vector2(3f, 0f) }, 1, Radius);

        _behaviour.Write(_steering, new Vector2(2f, 0f));

        Assert.Equal(0.5f, _steering.Danger.GetValue(RightSlot), Tolerance);
    }
}
