using System.Numerics;

namespace GroveGames.Steering.Tests;

public sealed class ContextSteeringAgentTests
{
    private const int AgentId = 0;
    private const int OtherAgentId = 2;
    private const int SlotCount = 16;
    private const float Tolerance = 1e-4f;
    private const float HalfLife = 0.05f;

    private readonly ContextSteering _steering;
    private readonly SeekBehaviour _seek;
    private readonly AvoidBehaviour _avoid;
    private readonly ContextSteeringAgent _agent;

    public ContextSteeringAgentTests()
    {
        _steering = new ContextSteering(SlotCount, 1f, 0.1f, 0f);
        _seek = new SeekBehaviour();
        _avoid = new AvoidBehaviour();
        _agent = new ContextSteeringAgent(AgentId, _steering, new IContextBehaviour[] { _seek, _avoid });
    }

    [Fact]
    public void SelectDirection_SeekOnly_ReturnsTheSeekDirection()
    {
        _seek.SetDirection(Vector2.UnitY, 1f);

        var direction = _agent.SelectDirection(Vector2.Zero);

        Assert.True((direction - Vector2.UnitY).Length() < Tolerance);
        Assert.Equal(1f, _agent.GetInterest(direction), Tolerance);
    }

    [Fact]
    public void SelectDirection_ObstacleOnTheWay_SteersAroundIt()
    {
        _seek.SetDirection(Vector2.UnitY, 1f);
        _avoid.SetObstacles(new[] { new Vector2(0f, 0.5f) }, 1, 2f);

        var direction = _agent.SelectDirection(Vector2.Zero);

        Assert.True(Math.Abs(direction.X) > 0.9f);
        Assert.True(_agent.GetInterest(direction) < 0.6f);
    }

    [Fact]
    public void SelectDirection_CalledAgain_StartsFromClearMaps()
    {
        _seek.SetDirection(Vector2.UnitY, 1f);
        _avoid.SetObstacles(new[] { new Vector2(0f, 0.5f) }, 1, 2f);
        _agent.SelectDirection(Vector2.Zero);
        _avoid.SetObstacles(new[] { new Vector2(0f, 0.5f) }, 0, 2f);

        var direction = _agent.SelectDirection(Vector2.Zero);

        Assert.True((direction - Vector2.UnitY).Length() < Tolerance);
    }

    [Fact]
    public void SelectDirection_NoInterest_ReturnsZero()
    {
        var direction = _agent.SelectDirection(Vector2.Zero);

        Assert.Equal(Vector2.Zero, direction);
    }

    [Fact]
    public void SelectDirection_NotRecording_RecordsNoSnapshot()
    {
        _seek.SetDirection(Vector2.UnitY, 1f);

        _agent.SelectDirection(Vector2.Zero);

        Assert.Empty(_steering.Snapshots);
    }

    [Fact]
    public void SelectDirection_Recording_RecordsMapsAndPoints()
    {
        var obstacle = new Vector2(1f, 0f);
        var point = new Vector2(0f, 5f);
        _steering.SetRecording(true);
        _seek.SetDirection(Vector2.UnitY, 1f);
        _seek.SetPoint(point);
        _avoid.SetObstacles(new[] { obstacle }, 1, 2f);

        var direction = _agent.SelectDirection(Vector2.One);

        var snapshot = _steering.Snapshots[AgentId];
        var upSlot = _steering.Interest.GetNearestSlot(Vector2.UnitY);
        Assert.True(snapshot.IsRecorded);
        Assert.Equal(Vector2.One, snapshot.Position);
        Assert.Equal(direction, snapshot.Direction);
        Assert.Equal(SlotCount, snapshot.SlotCount);
        Assert.Equal(_steering.Interest.GetValue(upSlot), snapshot.GetInterest(upSlot));
        Assert.Equal(_steering.Danger.GetValue(upSlot), snapshot.GetDanger(upSlot));
        Assert.Equal(new[] { point }, snapshot.InterestPoints);
        Assert.Equal(new[] { obstacle }, snapshot.DangerPoints);
    }

    [Fact]
    public void SelectDirection_RecordingObstacleOutOfRadius_RecordsNoDangerPoint()
    {
        _steering.SetRecording(true);
        _seek.SetDirection(Vector2.UnitY, 1f);
        _avoid.SetObstacles(new[] { new Vector2(5f, 0f) }, 1, 2f);

        _agent.SelectDirection(Vector2.Zero);

        Assert.Empty(_steering.Snapshots[AgentId].DangerPoints);
    }

    [Fact]
    public void SelectDirection_RecordingObstacleOnTheWay_RecordsTheSlotAsBlocked()
    {
        _steering.SetRecording(true);
        _seek.SetDirection(Vector2.UnitY, 1f);
        _avoid.SetObstacles(new[] { new Vector2(0f, 0.5f) }, 1, 2f);

        _agent.SelectDirection(Vector2.Zero);

        var snapshot = _steering.Snapshots[AgentId];
        Assert.True(snapshot.IsBlocked(_steering.Interest.GetNearestSlot(Vector2.UnitY)));
        Assert.False(snapshot.IsBlocked(_steering.Interest.GetNearestSlot(-Vector2.UnitY)));
    }

    [Fact]
    public void SelectDirection_NearlyEqualInterests_KeepsEachAgentsOwnPreviousDirection()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var up = new SeekBehaviour();
        var right = new SeekBehaviour();
        var first = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { up, right });
        var second = new ContextSteeringAgent(OtherAgentId, steering, new IContextBehaviour[] { up, right });
        up.SetDirection(Vector2.UnitY, 1f);
        first.SelectDirection(Vector2.Zero);
        first.Commit();
        up.SetDirection(Vector2.UnitY, 0f);
        right.SetDirection(Vector2.UnitX, 1f);
        second.SelectDirection(Vector2.Zero);
        second.Commit();
        first.CompleteFrame(HalfLife);
        second.CompleteFrame(HalfLife);
        up.SetDirection(Vector2.UnitY, 0.97f);

        var firstDirection = first.SelectDirection(Vector2.Zero);
        var secondDirection = second.SelectDirection(Vector2.Zero);

        Assert.Equal(steering.Interest.GetNearestSlot(Vector2.UnitY), steering.Interest.GetNearestSlot(firstDirection));
        Assert.Equal(steering.Interest.GetNearestSlot(Vector2.UnitX), steering.Interest.GetNearestSlot(secondDirection));
    }

    [Fact]
    public void SelectDirection_SecondQueryInTheSameFrame_BlendsWithTheCommittedFrameNotTheFirstQuery()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.Commit();
        agent.CompleteFrame(HalfLife);
        seek.SetDirection(-Vector2.UnitX, 1f);
        agent.SelectDirection(Vector2.Zero);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(0.5f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
        Assert.Equal(0f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(-Vector2.UnitX)), Tolerance);
    }

    [Fact]
    public void SelectDirection_AfterCompleteFrame_BlendsInTheLastSelection()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.Commit();
        agent.CompleteFrame(HalfLife);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(0.5f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
    }

    [Fact]
    public void CompleteFrame_WithoutCommit_ForgetsThePreviousSelection()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.CompleteFrame(HalfLife);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(0f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
    }

    [Fact]
    public void CompleteFrame_FrameWithoutCommitAfterACommittedFrame_ForgetsTheOldSelection()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.Commit();
        agent.CompleteFrame(HalfLife);
        agent.CompleteFrame(HalfLife);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(0f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
    }

    [Fact]
    public void CompleteFrame_NoTimePassed_KeepsAllOfThePreviousInterest()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.Commit();
        agent.CompleteFrame(0f);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(1f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
    }

    [Fact]
    public void CompleteFrame_ZeroHalfLife_KeepsNoPreviousInterest()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, 0f);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.Commit();
        agent.CompleteFrame(HalfLife);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(0f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
    }

    [Fact]
    public void CompleteFrame_TwoHalfLives_KeepsAQuarterOfThePreviousInterest()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.Commit();
        agent.CompleteFrame(HalfLife * 2f);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(0.25f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
    }

    [Fact]
    public void Reset_AfterACommittedFrame_ForgetsThePreviousSelection()
    {
        var steering = new ContextSteering(SlotCount, 0f, 0.1f, HalfLife);
        var seek = new SeekBehaviour();
        var agent = new ContextSteeringAgent(AgentId, steering, new IContextBehaviour[] { seek });
        seek.SetDirection(Vector2.UnitY, 1f);
        agent.SelectDirection(Vector2.Zero);
        agent.Commit();
        agent.CompleteFrame(HalfLife);
        seek.SetDirection(Vector2.UnitX, 1f);

        agent.Reset();
        agent.SelectDirection(Vector2.Zero);

        Assert.Equal(0f, steering.Interest.GetValue(steering.Interest.GetNearestSlot(Vector2.UnitY)), Tolerance);
    }

    [Fact]
    public void SelectDirection_RecordingTwoAgents_KeepsTheirSnapshotsApart()
    {
        _steering.SetRecording(true);
        _seek.SetDirection(Vector2.UnitY, 1f);

        _agent.SelectDirection(Vector2.Zero);
        new ContextSteeringAgent(OtherAgentId, _steering, new IContextBehaviour[] { _seek, _avoid }).SelectDirection(Vector2.One);

        Assert.Equal(Vector2.Zero, _steering.Snapshots[AgentId].Position);
        Assert.Equal(Vector2.One, _steering.Snapshots[OtherAgentId].Position);
    }
}
