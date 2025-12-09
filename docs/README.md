# Space Station Game - Documentation

This documentation describes the conceptual architecture of the Space Station Game project. The goal is to help you understand *how we think about the problem* rather than the technical implementation details—the code speaks for itself on that front.

## Contents

1. [Architecture Overview](./architecture-overview.md) - The big picture: how the pieces fit together
2. [Simulation Engine](./simulation-engine.md) - How we model time and run the game world
3. [Event System](./event-system.md) - How the simulation communicates what's happening
4. [Systems](./systems.md) - How game logic is organized and executed

## Design Philosophy

### Separation of Simulation and Presentation

The core principle driving this architecture is that **the simulation should know nothing about how it's presented**. The engine runs the game world—managing time, updating systems, and recording what happens. How that world gets shown to a player (or logged, or replayed, or sent over a network) is someone else's problem.

This separation gives us flexibility. The same simulation can drive a rich graphical interface, a text console, an automated test harness, or a network replay system. The simulation doesn't care. It just runs and tells anyone who's listening what happened.

### Determinism Through Fixed Timesteps

Games are easier to reason about when the same inputs always produce the same outputs. By running the simulation at a fixed rate (regardless of how fast the computer is), we get predictable, reproducible behavior. This matters for debugging, testing, multiplayer synchronization, and replay systems.

### Events as the Record of Truth

When something happens in the simulation, it emits an event. These events are the canonical record of what occurred. Channels receive these events and do whatever they need to—render graphics, play sounds, write logs, send network packets. The simulation doesn't know or care what the channels do with the information.
