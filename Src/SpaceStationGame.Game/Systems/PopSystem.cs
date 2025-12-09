using SpaceStationGame.Engine;
using SpaceStationGame.Engine.Events;
using SpaceStationGame.Engine.Systems;

namespace SpaceStationGame.Game;

public class PopSystem : ISystem
{

    public readonly EventBus EventBus;
    public PopSystem(EventBus eventBus)
    {
        EventBus = eventBus;
    }

    public void Update(double timeStepMs, CancellationToken cancellationToken)
    {
        EventBus.Emit(new PopUpdateEvent(timeStepMs));
    }
}

public record PopUpdateEvent(double timeStepMs) : IEventData
{
    public static int EventTypeId => 1;
}