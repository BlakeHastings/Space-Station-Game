# Simulation Engine

## The Problem of Time

Games need to model the passage of time, but computers run at varying speeds. A fast machine might execute the game loop thousands of times per second; a slow one might struggle to hit sixty. If we simply update the simulation every time through the loop, the game runs at different speeds on different machines. That's unacceptable.

## Fixed Timestep

The solution is to separate *real time* from *simulation time*. The simulation always advances in fixed increments—we call these "ticks." Each tick represents the same amount of simulated time, regardless of how long it took the computer to process.

Think of it like a film projector. The film always shows 24 frames per second, whether the projector is running smoothly or struggling. The audience sees consistent motion because the *content* is fixed, even if the *delivery* varies.

Our simulation runs at 60 ticks per second. Each tick represents approximately 16.67 milliseconds of simulated time. When the engine updates, it figures out how much real time has passed and runs enough ticks to catch up.

## The Accumulator

Here's how it works in practice:

1. The engine checks how much real time has passed since the last update
2. That time gets added to an "accumulator"
3. While the accumulator holds enough time for a tick, the engine runs a tick and subtracts the tick duration from the accumulator
4. Any leftover time stays in the accumulator for next time

This approach handles variable frame rates gracefully. If the computer is fast, we might run zero or one tick per frame. If it's slow, we might run several ticks to catch up. Either way, the simulation sees consistent, fixed-duration ticks.

## Handling Slowdowns

What if the computer falls so far behind that catching up would take too long? Running hundreds of ticks in a single frame would make things worse, not better. This is called the "spiral of death."

We prevent this by capping how much real time we'll process in a single update. If the computer falls more than 250 milliseconds behind, we simply drop the excess time. The simulation will run slightly slower than real-time during the slowdown, but it won't collapse into an unrecoverable state.

This is a tradeoff. We sacrifice some timing accuracy to maintain stability. For a game (as opposed to, say, a scientific simulation), this is the right choice.

## What Happens in a Tick

Each tick follows a consistent sequence:

1. **Systems update** - All registered game systems get a chance to run, advancing the game state
2. **Event bus updates** - The bus records the current tick number so events are properly timestamped
3. **Events flush** - All events emitted during the tick are delivered to channels

This order matters. Systems do their work first, potentially emitting events. Then those events get delivered. Channels always see a complete, consistent picture of what happened during the tick.

## Why This Matters

Fixed timesteps give us several important properties:

- **Determinism** - The same sequence of inputs always produces the same simulation state
- **Testability** - We can write tests that don't depend on real time passing
- **Fairness** - The game runs the same way for everyone, regardless of their hardware
- **Replayability** - We can record inputs and replay them to reproduce any game session

These properties become essential as the game grows more complex. They're worth the small additional complexity of the accumulator approach.
