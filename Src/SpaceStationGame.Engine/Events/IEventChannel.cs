namespace SpaceStationGame.Engine.Events;

public interface IEventChannel
{
    public void ReceiveBatch(List<EventBusEvent> events);
}