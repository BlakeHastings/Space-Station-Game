using SpaceStationGame.Engine;
using SpaceStationGame.Engine.Systems;
using Arch;
using Arch.Core;
using SpaceStationGame.Game.Components;
using SpaceStationGame.Engine.Events;

namespace SpaceStationGame.Game;

public class ProductionSystem : ISystem
{
    private readonly World _world;
    private readonly EventBus _eventBus;
    private readonly QueryDescription _productionQuery = new QueryDescription().WithAll<InventoryComponent, RecipeComponent, EnabledComponent>();

    public ProductionSystem(World world, EventBus eventBus)
    {
        _world = world;
        _eventBus = eventBus;
    }

    public void Update(double timeStepMs, CancellationToken cancellationToken)
    {
        _world.Query(_productionQuery, (Entity entity, ref InventoryComponent inventory, ref RecipeComponent recipe, ref EnabledComponent enabled) =>
        {
            _eventBus.Emit(new ProductionEvent(entity.Id));
            
            // read inventory
            // check for ingredients

            // if ingredients available, remove them and add products
 

        });

        Console.WriteLine($"{nameof(ProductionSystem)} Update Call {timeStepMs}");
    }
}

public record ProductionEvent(int EntityId) : IEventData
{
    public static int EventTypeId => 2;
}