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
                Console.WriteLine(" - contains resource: " + resource.ResourceName + " weighing " + resource.WeightKg + " kg");
            }

            // check for ingredients

            bool IngredientsAvailable = true;

            foreach (var item in recipe.Ingredients)
            {
                var resource = _world.Get<RecourseComponent>(item);
                Console.WriteLine(" - recepie needs  resource: " + resource.ResourceName + " weighing " + resource.WeightKg + " kg");

                // check if inventory has this ingredient
                var inventoryHasResource = inventory.EntityInventory.Any(invItem =>
                {
                    var invResource = _world.Get<RecourseComponent>(invItem);
                    var neededResource = _world.Get<RecourseComponent>(item);
                    return invResource.ResourceName == neededResource.ResourceName;
                });

                if (!inventoryHasResource)
                {
                    Console.WriteLine(" -- missing ingredient: " + resource.ResourceName);
                    IngredientsAvailable = false;
                } // check if inventory has enough quantity of this ingredient
                else
                {
                    // Inventory entity list is unique, so just get the weight directly @Savamen07   right this is ture ? #TODO: check if  this ture ?
                    var inventoryResource = _world.Get<RecourseComponent>(item);
                    
                    // Check if inventory has enough weight of this resource  
                    // this is ai slop i dotn even understand it 
                    double totalAvailable = inventory.EntityInventory.Where(invItem =>
                    {
                        var invResource = _world.Get<RecourseComponent>(invItem);

                        return invResource.ResourceName == resource.ResourceName;
                        
                    }).Sum(invItem => _world.Get<RecourseComponent>(invItem).WeightKg);

                if (totalAvailable >= resource.WeightKg)
                {
                    Console.WriteLine($" -- found enough! (have: {totalAvailable} kg, need: {resource.WeightKg} kg)");
                }
                                else
                                {
                                    Console.WriteLine(" -- insufficient quantity! (have: " + inventoryResource.WeightKg + " kg, need: " + resource.WeightKg + " kg)");
                                    IngredientsAvailable = false;
                                }
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