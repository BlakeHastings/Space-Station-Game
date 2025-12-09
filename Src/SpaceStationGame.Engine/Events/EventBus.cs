namespace SpaceStationGame.Engine.Events;

public interface IEventBus {
    public void Emit<T>(T eventData) where T : IEventData;
    public void Flush();
    public void Update(long currentTick, long currentTime);
    public void RegisterChannel(IEventChannel eventChannel);
}

public class EventBus : IEventBus
{
    public List<EventEnvelope> PendingEvents { get; } = [];
    public List<IEventChannel> Channels { get; } = [];
    public long CurrentTick { get; private set; }
    public long CurrentTime { get; private set; }

    public void Emit<T>(T eventData) where T : IEventData
    {
        var envelope = new EventEnvelope(CurrentTick, CurrentTime, T.EventTypeId, eventData);
        PendingEvents.Add(envelope);
    }

    public void Flush()
    {
        // no events to flush. lets get out of here
        if (PendingEvents.Count == 0) return;

        var eventsToSend = new List<EventEnvelope>(PendingEvents);
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

    public void RegisterChannel(IEventChannel eventChannel)
    {
        Channels.Add(eventChannel);
    }
}
