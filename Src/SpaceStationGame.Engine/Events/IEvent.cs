namespace SpaceStationGame.Engine.Events;

// this probably isn't correct but it satisfies the type gods in the EventBus. So good enough for now.
public abstract class EventBusEvent(long tick, long simulationTime)
{
    public readonly long Tick = tick;
    public readonly long SimulationTime = simulationTime;
}

public class Event<T>(long tick, long simulationTime, T eventData) : EventBusEvent(tick, simulationTime)
{
    public readonly T EventData = eventData;
}