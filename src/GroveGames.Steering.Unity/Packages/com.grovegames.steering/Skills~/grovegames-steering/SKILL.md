---
name: grovegames-steering
description: Rules for moving agents with context steering in game code with GroveGames.Steering. Use when writing or reviewing code that creates a ContextSteeringAgent, writes a seek or avoid behaviour, selects a movement direction, or adds a new IContextBehaviour.
---

# Using GroveGames.Steering

Rules for game code that uses this package. They hold in every game. The API is in the package's `README.md`.

- One `IContextSteering` is shared by all agents. Create one `ContextSteeringAgent` per moving thing, with its own behaviour instances, and reuse it when the thing is pooled; call `Reset` when it is reused. Give agents small ids counted from 0, one per agent; the id indexes the snapshot list.
- Every frame, for every agent: set the behaviours, call `SelectDirection`, call `Commit` only if the agent really moves that way, and call `CompleteFrame` once at the end of the frame. Skipping `CompleteFrame` freezes the blending.
- Call `GetInterest` right after the same agent's `SelectDirection`; it reads the shared maps, which the next agent overwrites.
- `SelectDirection` can return zero. Treat zero as "stay", never normalize it.
- Interest says where the agent wants to go; danger says where it must not. Never encode avoidance as negative interest.
- Reuse the obstacle array given to `AvoidBehaviour.SetObstacles`. Fill it and pass the count; never allocate one per frame.
- A new rule for where to go or what to avoid is a new `IContextBehaviour`. It writes into `steering.Interest` or `steering.Danger` in `Write`, and adds what it looked at in `WritePoints`. Do not change the selected direction after the fact in game code.
- Tune slot count, spread, danger tolerance and half-life in the settings, never in code.

## Unity

- The algorithm is in the `GroveGames.Steering` namespace, on `System.Numerics.Vector2`. `GroveGames.Steering.Unity` holds `UnityContextSteeringSettings`, the asset that implements `IContextSteeringSettings`, and the registration.
- Convert vectors once at the edge of the steering code: `new System.Numerics.Vector2(position.x, position.y)` going in, `new Vector2(direction.X, direction.Y)` coming out. Keep obstacle arrays as `System.Numerics.Vector2[]`.
- Register with `builder.AddContextSteering(settings)` in the installer that owns the agents, with a `UnityContextSteeringSettings` as a serialized field.
- An asmdef that overrides references lists `GroveGames.Steering.dll` in its precompiled references.
