# GroveGames.Steering

[![Build Status](https://github.com/grovegs/Steering/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/Steering/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/Steering/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/Steering/actions/workflows/tests.yml)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.Steering)](https://www.nuget.org/packages/GroveGames.Steering)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/Steering)](https://github.com/grovegs/Steering/releases/latest)

Context steering that picks a movement direction from interest and danger maps, for .NET and Unity

## Features

- `ContextSteering`: an interest map and a danger map over evenly spaced directions, and the best open direction between them
- `ContextSteeringAgent`: one per moving thing; it runs the behaviours, blends interest over time so the direction does not flicker, and records what it decided
- `SeekBehaviour` and `AvoidBehaviour`, and `IContextBehaviour` for your own
- `ContextSteeringSnapshot`: the maps and points of one decision, for drawing them
- Everything is on `System.Numerics.Vector2` and allocates nothing per frame
- Unity: a settings asset and a debug drawer for the developer tools

## .NET

### Installation

```bash
dotnet add package GroveGames.Steering
```

### Usage

```csharp
using System.Numerics;
using GroveGames.Steering;

var steering = new ContextSteering(slotCount: 16, interestSpread: 1f, dangerTolerance: 0.1f, interestHalfLife: 0.05f);
var seek = new SeekBehaviour();
var avoid = new AvoidBehaviour();
var agent = new ContextSteeringAgent(id: 0, steering, new IContextBehaviour[] { seek, avoid });

seek.SetDirection(target - position, strength: 1f);
avoid.SetObstacles(obstacles, obstacleCount, radius: 2f);

var direction = agent.SelectDirection(position);
agent.Commit();
agent.CompleteFrame(deltaTime);
```

- One `ContextSteering` is shared by all agents. Each agent clears it, writes into it and reads its answer inside `SelectDirection`.
- `SelectDirection` returns a unit direction, or zero when no open direction has interest.
- Call `Commit` when the agent really moves in the selected direction, and `CompleteFrame` once per frame for every agent. Only committed interest is blended into the next frame.
- While `SetRecording(true)` is on, each agent fills its `ContextSteeringSnapshot`. Whoever draws a snapshot checks `IsRecorded` and calls `Clear()` afterwards, so a decision is drawn once.
- `Reset` forgets the blended interest, for an agent that is reused.

## Unity

### Installation

1. Install the core package via [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity):
   - Open Window > NuGet > Manage NuGet Packages
   - Search for `GroveGames.Steering` and install

2. Add the Unity package via Git URL:
   - Open Package Manager (Window > Package Manager)
   - Click "+" > "Add package from git URL"
   - Enter: `https://github.com/grovegs/Steering.git?path=src/GroveGames.Steering.Unity/Packages/com.grovegames.steering`

3. Create a `csc.rsp` file in your `Assets/` directory with the following content to enable C# 10 features:

   ```text
   -langversion:10
   -nullable:enable
   ```

### Usage

Create the settings with Assets > Create > GroveGames > Steering > Context Steering Settings. With [GroveGames.DependencyInjection](https://github.com/grovegs/DependencyInjection) installed:

```csharp
using GroveGames.Steering.Unity;

builder.AddContextSteering(_contextSteeringSettings);
```

- Positions and directions are `System.Numerics.Vector2`. Convert at the edge: `new System.Numerics.Vector2(position.x, position.y)` going in, `new Vector2(direction.X, direction.Y)` coming out.
- With [GroveGames.DeveloperTools](https://github.com/grovegs/DeveloperTools) installed, the Steering options have a "Show Maps" toggle that draws every agent's maps in the scene.

## Development

### Project Structure

- `src/GroveGames.Steering/` - Core .NET library (`net10.0`, `netstandard2.1`)
- `src/GroveGames.Steering.Unity/` - Unity package
- `tests/` - xUnit tests
- `sandbox/` - Sample applications for testing

### Commands

```bash
dotnet build                        # Build all projects
dotnet format                       # Format code
dotnet test                         # Run tests
dotnet pack -c Release              # Create NuGet packages
```

### Requirements

- .NET 10.0 SDK

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
