using System.Numerics;

namespace GroveGames.Steering.Tests;

public sealed class ContextSteeringSnapshotTests
{
    private const int SlotCount = 8;

    [Fact]
    public void Constructor_NothingRecorded_IsNotRecorded()
    {
        var snapshot = new ContextSteeringSnapshot(SlotCount);

        Assert.False(snapshot.IsRecorded);
        Assert.Equal(SlotCount, snapshot.SlotCount);
    }

    [Fact]
    public void Record_Decision_IsRecordedWithItsValues()
    {
        var steering = new ContextSteering(SlotCount, 1f, 0.1f, 0f);
        var snapshot = new ContextSteeringSnapshot(SlotCount);
        steering.Interest.Write(Vector2.UnitX, 1f);

        snapshot.Record(steering, Vector2.One, Vector2.UnitX);

        Assert.True(snapshot.IsRecorded);
        Assert.Equal(Vector2.One, snapshot.Position);
        Assert.Equal(Vector2.UnitX, snapshot.Direction);
        Assert.Equal(steering.Interest.GetValue(0), snapshot.GetInterest(0));
    }

    [Fact]
    public void Record_AfterPointsWereAdded_DropsTheOldPoints()
    {
        var steering = new ContextSteering(SlotCount, 1f, 0.1f, 0f);
        var snapshot = new ContextSteeringSnapshot(SlotCount);
        snapshot.AddInterestPoint(Vector2.One);
        snapshot.AddDangerPoint(Vector2.One);

        snapshot.Record(steering, Vector2.Zero, Vector2.Zero);

        Assert.Empty(snapshot.InterestPoints);
        Assert.Empty(snapshot.DangerPoints);
    }

    [Fact]
    public void Clear_RecordedSnapshot_IsNotRecordedAndHasNoPoints()
    {
        var steering = new ContextSteering(SlotCount, 1f, 0.1f, 0f);
        var snapshot = new ContextSteeringSnapshot(SlotCount);
        snapshot.Record(steering, Vector2.One, Vector2.UnitX);
        snapshot.AddInterestPoint(Vector2.One);
        snapshot.AddDangerPoint(Vector2.One);

        snapshot.Clear();

        Assert.False(snapshot.IsRecorded);
        Assert.Empty(snapshot.InterestPoints);
        Assert.Empty(snapshot.DangerPoints);
    }
}
