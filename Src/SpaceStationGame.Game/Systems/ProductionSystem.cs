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
    private readonly QueryDescription _productionQuery = new QueryDescription().WithAll<InventoryComponent, RecipeComponent,NameComponent, EnabledComponent>();

    public ProductionSystem(World world, EventBus eventBus)
    {
        _world = world;
        _eventBus = eventBus;
    }

    public void Update(double timeStepMs, CancellationToken cancellationToken)
    {
        _world.Query(_productionQuery, (Entity entity, ref InventoryComponent inventory, ref RecipeComponent recipe, ref EnabledComponent enabled,ref NameComponent name1) =>
        {
            _eventBus.Emit(new ProductionEvent(entity.Id, inventory.Capacity));
            _eventBus.Emit(new NameEvent(name1.Name_1234));
            
            // read inventory
            Console.WriteLine("Inventory of"+ name1.Name_1234 + " has capacity: " + inventory.Capacity);
            Console.WriteLine("with items: " + inventory.EntityInventory);
            foreach (var item in inventory.EntityInventory)
            {
                var resource = _world.Get<RecourseComponent>(item);
                var resourceType = _world.Get<RecourseTypeComponent>(resource.RecourseTypeComponent);
                Console.WriteLine(" - contains resource: " + resourceType.RecourseName + " weighing " + resource.WeightKg + " kg");
            }

            // check for ingredients

            

            foreach (var item in recipe.Ingredients)
            {
                var resource = _world.Get<RecourseComponent>(item);
                var resourceType = _world.Get<RecourseTypeComponent>(resource.RecourseTypeComponent);
                Console.WriteLine(" - recepie needs  resource: " + resourceType.RecourseName + " weighing " + resource.WeightKg + " kg");

                // check if inventory has this ingredient #TODO: @Savaman07  check if this is efficnet enough 
                bool IngredientsAvailable = inventory.EntityInventory.Any(invItem =>
                {
                    var invResource = _world.Get<RecourseComponent>(invItem);
                    var neededResource = _world.Get<RecourseComponent>(item);
                    if (invResource.RecourseTypeComponent == neededResource.RecourseTypeComponent)
                    { 
                        return invResource.WeightKg >= neededResource.WeightKg;
                    }
                    else return false;
                    
                });


                // just debug prints   #TODO:  remove debug prints 
                if (!IngredientsAvailable)
                {

                    
                    Console.WriteLine(" -- missing ingredient: " + _world.Get<RecourseTypeComponent>(resource.RecourseTypeComponent).RecourseName);
                    
                }
                else
                {
                    Console.WriteLine(" -- ingredient found in inventory.");

                    }
                

            }

            // if ingredients available, remove them and add products
           
 

        });

        Console.WriteLine($"{nameof(ProductionSystem)} Update Call {timeStepMs}");
    }
}

public record ProductionEvent(int EntityId,int inventoryCap) : IEventData
{
    public static int EventTypeId => 2;
}

public record NameEvent(string name) : IEventData
{
    public static int EventTypeId => 3;
}