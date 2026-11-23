using System.Collections.Generic;
using System.Numerics;

namespace GroveGames.Steering;

public interface IContextSteering
{
    public ContextMap Interest { get; }
    public ContextMap Danger { get; }
    public float MaximumDanger { get; }
    public float InterestHalfLife { get; }
    public bool IsRecording { get; }
    public IReadOnlyList<ContextSteeringSnapshot> Snapshots { get; }

    public void Clear();
    public Vector2 SelectDirection();
    public float GetInterest(Vector2 direction);
    public void SetRecording(bool isRecording);
    public ContextSteeringSnapshot GetSnapshot(int agentId);
}
