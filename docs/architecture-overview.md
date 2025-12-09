# Architecture Overview

## The Three Layers

The project is organized into three distinct layers, each with a clear responsibility:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation                         │
│              (Console, GUI, Network, etc.)              │
└─────────────────────────┬───────────────────────────────┘
                          │ Events flow out via Channels
                          ▼
┌─────────────────────────────────────────────────────────┐
│                      Game Logic                         │
│                (Systems, Game Rules)                    │
└─────────────────────────┬───────────────────────────────┘
                          │ Systems register with Engine
                          ▼
┌─────────────────────────────────────────────────────────┐
│                   Simulation Engine                     │
│            (Time, Scheduling, Event Bus)                │
└─────────────────────────────────────────────────────────┘
```

### Simulation Engine

The foundation. It manages the flow of time, decides when systems should update, and provides the event bus for communication. The engine knows nothing about space stations, populations, or resources—it only knows about time and coordination.

### Game Logic

The rules of the game. Systems here implement the actual mechanics: population growth, resource production, life support, and so on. Each system focuses on one aspect of the simulation and runs at whatever rate makes sense for its purpose.

### Presentation

How players (or other consumers) experience the simulation. A channel might render events to a graphical display, print them to a console, write them to a log file, or send them across a network. Multiple channels can observe the same simulation simultaneously.

## Information Flow

Information flows in one direction: **outward from the simulation**.

1. The engine advances time
2. Systems update and emit events describing what happened
3. The event bus collects events for the current tick
4. At the end of each tick, events are flushed to all registered channels
5. Channels process events however they see fit

This one-way flow is intentional. The simulation is the source of truth. Presentation layers observe but don't influence. Player input, when implemented, will flow through a separate command system that the simulation processes on its own terms.

## Project Structure

The codebase reflects these layers:

- **SpaceStationGame.Engine** - The simulation engine, event bus, and system scheduler. No game-specific concepts live here.
- **SpaceStationGame.Game** - Game systems and channel implementations. This is where space station mechanics live.
- **SpaceStationGame.Console** - A simple console-based runner that wires everything together and starts the simulation.

This structure means the engine could be reused for an entirely different game. The game logic could be presented through any number of different interfaces. Each piece is independent and replaceable.
