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

    public readonly double minRecourseWeightKg = 0.0001; // = 0.1 gram

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
            var recipeSpeedMultiplier = recipe.SpeedMultiplier; // so we need this cause recipe is not available in lambdas or something 
            var recipeEfficiencyMultiplier = recipe.EfficiencyMultiplier;
            
            foreach (var item in inventory.EntityInventory)
            {
                var resource = _world.Get<RecourseComponent>(item);
                var resourceType = _world.Get<RecourseTypeComponent>(resource.RecourseTypeComponent);
                Console.WriteLine(" - contains resource: " + resourceType.RecourseName + " weighing " + resource.WeightKg + " kg");
            }

            // check for ingredients

            bool IngredientsAvailable = true;

            foreach (var item in recipe.Ingredients)
            {
                var resource = _world.Get<RecourseComponent>(item);
                var resourceType = _world.Get<RecourseTypeComponent>(resource.RecourseTypeComponent);

                Console.WriteLine(" - recipe needs  resource: " + resourceType.RecourseName + " weighing " + resource.WeightKg* recipeSpeedMultiplier + " kg");
                Console.WriteLine("recipe base ingredient :"+resource.WeightKg+ " kg");

                // check if inventory has this ingredient #TODO: @Savaman07  check if this is efficient enough 
                var neededResource = _world.Get<RecourseComponent>(item);

                var inventoryHasResource = inventory.EntityInventory.Any(invItem =>
                {
                    var invResource = _world.Get<RecourseComponent>(invItem);
                    if (invResource.RecourseTypeComponent == neededResource.RecourseTypeComponent)
                    { 
                        return invResource.WeightKg >= neededResource.WeightKg* recipeSpeedMultiplier;
                    }
                    else return false;
                    
                });

                
                // only debug output for now #TODO: remove these prints  and enable the break for performance
                if (!inventoryHasResource)
                {
                    Console.WriteLine(" -- missing ingredient: " + _world.Get<RecourseTypeComponent>(resource.RecourseTypeComponent).RecourseName);
                    IngredientsAvailable = false;
                    //break; TODO: uncomment this for performance when removing debug prints
                }
                else
                {
                    Console.WriteLine(" -- ingredient found in inventory.");

                    }
                

            }

            if (!(IngredientsAvailable && enabled.IsEnabled))
            {
                Console.WriteLine("Cannot produce products. Missing ingredients or system disabled.");
            }
            else
            {
                Console.WriteLine("All ingredients available. Producing products...");
                Console.WriteLine("Producing products for recipe on entity: " + name1.Name_1234);
                Console.WriteLine("Recipe speedmulti = " + recipe.SpeedMultiplier + " effince multi ="+recipe.EfficiencyMultiplier);

                // produce products
                // aka add products to inventory
                foreach (var product in recipe.Products)
                {
                    var resource_product = _world.Get<RecourseComponent>(product);
                    var resourceType_product = _world.Get<RecourseTypeComponent>(resource_product.RecourseTypeComponent);

                     var inventoryHasResource = inventory.EntityInventory.FirstOrDefault(invItem =>
                    {
                        var invResource = _world.Get<RecourseComponent>(invItem);
                        return invResource.RecourseTypeComponent == resource_product.RecourseTypeComponent;
                    
                    });


                
                    if(inventoryHasResource.Id !=0)
                    { // resource type already in inventory

                    
                        var invResourceComp = _world.Get<RecourseComponent>(inventoryHasResource);
                        invResourceComp.WeightKg += resource_product.WeightKg* recipeEfficiencyMultiplier* recipeSpeedMultiplier;
                        _world.Set(inventoryHasResource, invResourceComp);

                        Console.WriteLine("adding weight to exiting recourse comp kg :"+invResourceComp.WeightKg);
                    
                    }
                
                    else
                    { // new resource type for inventory

                    
                        var newResourceEntity = _world.Create(new RecourseComponent
                        {
                            RecourseTypeComponent = resource_product.RecourseTypeComponent,
                            WeightKg = resource_product.WeightKg* recipeEfficiencyMultiplier* recipeSpeedMultiplier
                        });

                        // add to inventory      
                        inventory.EntityInventory.Append(newResourceEntity);  // #TODO: @Savaman07 check if we can jsut append like this 
                        Console.WriteLine("adding new recourse to inventory :"+ resourceType_product.RecourseName);
                        
                    }

                    // remove ingredients from inventory


                        /*              // #TODO: @Savaman07 does a dictionary make sense ?  as we have never more then  1 of each resourceType in inventory ?
                    // Build a lookup: RecourseTypeComponentId -> RecourseComponent in inventory
                            var inventoryResourcesByType = inventory.EntityInventory
                                .Select(invItem => _world.Get<RecourseComponent>(invItem))
                                .ToDictionary(r => r.RecourseTypeComponent); */
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        var resourceToRemove = _world.Get<RecourseComponent>(ingredient);

                        foreach (var invItem in inventory.EntityInventory)
                        {
                            var invResource = _world.Get<RecourseComponent>(invItem);
                            if (invResource.RecourseTypeComponent == resourceToRemove.RecourseTypeComponent)
                            {
                                if (invResource.WeightKg - resourceToRemove.WeightKg *recipeSpeedMultiplier < minRecourseWeightKg)
                                {
                                    _world.Destroy(invItem);
                                }
                                else
                                {
                                    invResource.WeightKg -= resourceToRemove.WeightKg * recipeSpeedMultiplier;
                                    _world.Set(invItem, invResource);
                                    Console.WriteLine("removed ingredient from inventory: " + _world.Get<RecourseTypeComponent>(invResource.RecourseTypeComponent).RecourseName + " new weight kg: " + invResource.WeightKg);
                                    break; // exit inner loop once we've found and updated the resource
                                }
                                
                            }
                        }


                    }
                }
            }
           
           
 

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