using GroveGames.DependencyInjection;
using GroveGames.DeveloperTools.Unity;

namespace GroveGames.Steering.Unity
{
    public static class SteeringDeveloperToolsContainerBuilderExtensions
    {
        public static IContainerBuilder AddContextSteeringDeveloperOptions(this IContainerBuilder builder)
        {
            builder.AddSingleton<ContextSteeringDebugDrawer>();
            return builder.AddDeveloperOptions<ContextSteeringDeveloperOptions>();
        }
    }
}
