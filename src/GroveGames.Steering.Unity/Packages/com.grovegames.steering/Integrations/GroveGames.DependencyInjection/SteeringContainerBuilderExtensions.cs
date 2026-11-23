using GroveGames.DependencyInjection;

namespace GroveGames.Steering.Unity
{
    public static class SteeringContainerBuilderExtensions
    {
        public static IContainerBuilder AddContextSteering(this IContainerBuilder builder, UnityContextSteeringSettings settings)
        {
            builder.AddSingleton(settings);
            builder.AddSingleton<IContextSteeringSettings>(settings);
            builder.AddSingleton<IContextSteering>(_ => new ContextSteering(settings));
#if GROVEGAMES_DEVELOPERTOOLS
            builder.AddContextSteeringDeveloperOptions();
#endif
            return builder;
        }
    }
}
