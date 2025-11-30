using SpaceStationGame.Engine.Events;

namespace SpaceStationGame.Game.Channels;

public class ConsoleChannel : IEventChannel
{



    public void ReceiveBatch(List<EventBusEvent> events)
    {
        foreach(var @event in events)
            Console.WriteLine(@event);    
    }
}

