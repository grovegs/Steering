## Unity

- The algorithm is in the `GroveGames.Steering` namespace, on `System.Numerics.Vector2`. `GroveGames.Steering.Unity` holds `UnityContextSteeringSettings`, the asset that implements `IContextSteeringSettings`, and the registration.
- Convert vectors once at the edge of the steering code: `new System.Numerics.Vector2(position.x, position.y)` going in, `new Vector2(direction.X, direction.Y)` coming out. Keep obstacle arrays as `System.Numerics.Vector2[]`.
- Register with `builder.AddContextSteering(settings)` in the installer that owns the agents, with a `UnityContextSteeringSettings` as a serialized field.
- An asmdef that overrides references lists `GroveGames.Steering.dll` in its precompiled references.
