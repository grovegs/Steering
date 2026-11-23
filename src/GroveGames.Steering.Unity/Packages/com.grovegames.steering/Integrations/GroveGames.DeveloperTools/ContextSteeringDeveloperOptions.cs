using GroveGames.DeveloperTools;

namespace GroveGames.Steering.Unity
{
    public sealed class ContextSteeringDeveloperOptions : IDeveloperOptions
    {
        private readonly ContextSteeringDebugDrawer _debugDrawer;

        [DeveloperOption]
        private bool _showMaps;

        public string Category => "Steering";

        public ContextSteeringDeveloperOptions(ContextSteeringDebugDrawer debugDrawer)
        {
            _debugDrawer = debugDrawer;
        }

        private void OnValidate()
        {
            _debugDrawer.SetVisible(_showMaps);
        }
    }
}
