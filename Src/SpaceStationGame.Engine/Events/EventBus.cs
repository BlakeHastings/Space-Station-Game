namespace SpaceStationGame.Engine.Events;

public class EventBus
{
    public List<EventBusEvent> PendingEvents {get; } = [];
    public List<IEventChannel> Channels {get; } = [];
    public long CurrentTick {get; private set; }
    public long CurrentTime {get; private set; }

    public void Emit<T>(T eventData)
    {
        var @event = new Event<T>(CurrentTick, CurrentTime, eventData);
        PendingEvents.Add(@event);
    }

    public void Flush()
    {
        // no events to flush. lets get out of here
        if (PendingEvents.Count == 0) return;

        var eventsToSend = new List<EventBusEvent>(PendingEvents);
        PendingEvents.Clear();

        foreach (var channel in Channels)
        {
            channel.ReceiveBatch(eventsToSend);
        }

    }

    public void Update(long currentTick, long currentTime)
    {
        CurrentTick = currentTick;
        CurrentTime = currentTime;
    }

}