# Event System

## The Communication Problem

The simulation does interesting things: populations grow, resources get produced, systems fail. Something needs to know about these happenings—maybe a renderer needs to show an animation, or a sound system needs to play an alert, or a log needs to record the event for later analysis.

The naive approach is to have the simulation call directly into these systems. When population grows, call the renderer. When a system fails, call the sound player. This creates tight coupling. The simulation now depends on every system that cares about its events. Adding a new observer means modifying the simulation. Testing becomes difficult because you can't run the simulation without all its dependents.

## Events and Channels

We solve this with indirection. Instead of calling observers directly, the simulation emits events—small packets of data describing what happened. These events go into the event bus. Observers register as "channels" with the event bus and receive events without the simulation knowing they exist.

```
Simulation                    Event Bus                    Channels
    │                            │                            │
    │  emit(PopulationGrew)      │                            │
    │ ────────────────────────▶  │                            │
    │                            │                            │
    │  emit(ResourceProduced)    │                            │
    │ ────────────────────────▶  │                            │
    │                            │                            │
    │         flush()            │                            │
    │ ────────────────────────▶  │  ReceiveBatch([events])   │
    │                            │ ────────────────────────▶  │
    │                            │                            │
```

The simulation only knows about the event bus. Channels only know about the event bus. Neither knows the other exists. This is loose coupling.

## Event Envelopes

Raw event data isn't quite enough. We also need context: when did this happen? What kind of event is it? The event bus wraps each event in an "envelope" containing:

- **Tick** - Which simulation tick this event occurred on
- **Simulation Time** - The timestamp within the simulation
- **Event Type ID** - What kind of event this is
- **Event Data** - The actual payload

This metadata lets channels make decisions. A replay system might use the tick number for synchronization. A logging channel might format the timestamp for human readers. A network channel might use the type ID for efficient serialization.

## Batching and Flushing

Events aren't delivered immediately. They accumulate during a tick, then get delivered all at once when the tick completes. This batching serves several purposes:

- **Consistency** - Channels see all events from a tick together, not interleaved with events from other ticks
- **Efficiency** - One delivery call per tick instead of one per event
- **Ordering** - Events within a tick maintain their emission order

The flush happens at a well-defined point in the tick cycle, after all systems have updated. Channels can rely on receiving a complete picture of what happened.

## Multiple Channels

Any number of channels can register with the event bus. All channels receive all events. Each channel decides independently what to do with them:

- A **console channel** might print events as JSON for debugging
- A **render channel** might update graphics based on state changes
- A **audio channel** might play sounds for certain event types
- A **network channel** might serialize events for transmission to other players
- A **recording channel** might save events to disk for replay

The simulation doesn't change regardless of how many channels are listening or what they're doing. Add channels, remove them, swap implementations—the simulation keeps running unchanged.

## Why Not Direct Callbacks?

You might wonder why we don't just use callback functions. The channel approach adds a layer of indirection that callbacks don't require.

The answer is about boundaries and lifecycles. Callbacks create direct references between the simulation and observers. Those references complicate testing, make it harder to reason about dependencies, and create opportunities for observers to accidentally affect the simulation.

Channels enforce a clean boundary. Data flows out, nothing flows back. The simulation can be tested in isolation. Channels can be developed and tested independently. The architecture remains comprehensible as the system grows.
