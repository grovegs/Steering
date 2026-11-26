using System;
using System.Numerics;

namespace GroveGames.Steering.Tests;

public sealed class ContextSteeringAgentAllocationTests
{
    private const int FrameCount = 100;
    private const float DeltaTime = 0.016f;

    private readonly ContextSteering _steering;
    private readonly SeekBehaviour _seek;
    private readonly AvoidBehaviour _avoid;
    private readonly Vector2[] _obstacles;
    private readonly ContextSteeringAgent _agent;

    public ContextSteeringAgentAllocationTests()
    {
        _steering = new ContextSteering(16, 1f, 0.1f, 0.05f);
        _seek = new SeekBehaviour();
        _avoid = new AvoidBehaviour();
        _obstacles = [new Vector2(-1f, 0f), new Vector2(0f, -1f)];
        _agent = new ContextSteeringAgent(0, _steering, [_seek, _avoid]);
    }

    [Fact]
    public void SteeringFrame_AfterWarmUp_DoesNotAllocate()
    {
        Run();

        var allocated = Measure(Run);

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void SteeringFrame_WhileRecording_DoesNotAllocate()
    {
        _steering.SetRecording(true);
        Run();

        var allocated = Measure(Run);

        Assert.Equal(0, allocated);
    }

    private void Run()
    {
        for (var i = 0; i < FrameCount; i++)
        {
            _agent.CompleteFrame(DeltaTime);
            _seek.SetDirection(new Vector2(1f, 0.3f), 1f);
            _seek.SetPoint(Vector2.One);
            _avoid.SetObstacles(_obstacles, _obstacles.Length, 2f);
            var direction = _agent.SelectDirection(Vector2.Zero);
            _agent.Commit();
            _agent.GetInterest(direction);
            _agent.GetInterest(new Vector2(0f, 1f));
            _steering.GetSnapshot(0).Clear();
        }
    }

    private static long Measure(Action action)
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
