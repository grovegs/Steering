using System.Numerics;

namespace GroveGames.Steering;

public interface IContextBehaviour
{
    public void Write(IContextSteering steering, Vector2 position);
    public void WritePoints(ContextSteeringSnapshot snapshot);
}
