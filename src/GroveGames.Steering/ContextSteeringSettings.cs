namespace GroveGames.Steering;

public sealed class ContextSteeringSettings : IContextSteeringSettings
{
    private readonly int _slotCount;
    private readonly float _interestSpread;
    private readonly float _dangerTolerance;
    private readonly float _interestHalfLife;

    public int SlotCount => _slotCount;
    public float InterestSpread => _interestSpread;
    public float DangerTolerance => _dangerTolerance;
    public float InterestHalfLife => _interestHalfLife;

    public ContextSteeringSettings(int slotCount, float interestSpread, float dangerTolerance, float interestHalfLife)
    {
        _slotCount = slotCount;
        _interestSpread = interestSpread;
        _dangerTolerance = dangerTolerance;
        _interestHalfLife = interestHalfLife;
    }
}
