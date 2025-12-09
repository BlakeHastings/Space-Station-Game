# Systems

## What is a System?

A system is a self-contained piece of game logic that updates over time. It focuses on one aspect of the simulation: population dynamics, resource production, life support, power distribution, and so on.

Systems don't know about each other directly. They interact through shared game state and events. This isolation makes each system easier to understand, test, and modify without breaking unrelated functionality.

## The System Scheduler

Not every system needs to run at the same rate. Population might change slowly and only need updates a few times per second. Physics might need updates every tick. A daily cycle system might only run once per simulated day.

The system scheduler manages this. Each system registers with a desired update rate (updates per second). The scheduler tracks time for each system independently and calls update when appropriate.

```
Engine Tick (60 Hz)
    │
    ├── Physics System (60 Hz) ──────── updates every tick
    │
    ├── Production System (4 Hz) ────── updates every ~15 ticks
    │
    └── Population System (2 Hz) ────── updates every ~30 ticks
```

This flexibility lets us match update rates to actual needs. Fast-changing state gets frequent updates. Slow-changing state doesn't waste cycles updating when nothing meaningful has changed.

## System Independence

Each system runs in its own little world. When update is called, the system:

1. Reads whatever game state it needs
2. Performs its calculations
3. Modifies game state as appropriate
4. Emits events describing what happened

Systems don't call each other. If the production system produces resources that the population system consumes, they coordinate through shared state, not direct communication. The production system updates resource counts; the population system reads those counts. Neither knows the other exists.

This independence has practical benefits:

- **Testing** - Test one system without instantiating others
- **Reasoning** - Understand one system without understanding all of them
- **Modification** - Change one system without breaking others
- **Parallelization** - Potentially run independent systems concurrently (future optimization)

## Update Timing

When a system's update method is called, it receives the timestep—how much simulated time this update represents. For a system running at 4 Hz, each update covers 250 milliseconds of simulated time.

The system uses this timestep to scale its calculations. If population grows at 1% per second and the timestep is 250ms, this update should grow population by 0.25%. This keeps simulation behavior consistent regardless of update rate.

The timestep is always fixed for a given system. A 4 Hz system always receives 250ms timesteps, never more, never less. This predictability simplifies the math and eliminates a class of timing bugs.

## Graceful Shutdown

Systems receive a cancellation token with each update. When the simulation needs to stop—user quit, test completed, error condition—the token signals cancellation. Well-behaved systems check this token and exit cleanly when signaled.

This matters for systems that might take a while to update. A pathfinding system searching a large graph should check the token periodically and abandon the search if cancelled. Ignoring the token delays shutdown and creates a poor user experience.

## Future Considerations

The current system design is intentionally simple. As the game grows, we might need:

- **Dependencies** - Some systems might need to run before others
- **Phases** - Group systems into logical phases (input, simulation, output)
- **Priorities** - Ensure critical systems run even when the simulation is overloaded
- **State snapshots** - Capture system state for save/load functionality

These additions would layer onto the current design without replacing it. The core concept—independent units of logic updating at their own rates—remains sound.
