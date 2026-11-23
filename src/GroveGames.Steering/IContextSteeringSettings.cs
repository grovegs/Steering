namespace GroveGames.Steering;

public interface IContextSteeringSettings
{
    public int SlotCount { get; }
    public float InterestSpread { get; }
    public float DangerTolerance { get; }
    public float InterestHalfLife { get; }
}
