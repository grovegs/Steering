using GroveGames.DependencyInjection;

namespace GroveGames.Steering.Unity
{
    public static class SteeringContainerBuilderExtensions
    {
        public static IContainerBuilder AddContextSteering(this IContainerBuilder builder, ContextSteeringSettings settings)
        {
            builder.AddSingleton(settings);
            builder.AddSingleton<IContextSteering>(_ => new ContextSteering(settings.SlotCount, settings.InterestSpread, settings.DangerTolerance, settings.InterestHalfLife));
#if GROVEGAMES_DEVELOPERTOOLS
            builder.AddContextSteeringDeveloperOptions();
#endif
            return builder;
        }
    }
}
