// See https://aka.ms/new-console-template for more information
using SpaceStationGame.Engine;
using SpaceStationGame.Engine.Events;
using SpaceStationGame.Engine.Systems;
using SpaceStationGame.Game;
using SpaceStationGame.Game.Channels;
using Arch;
using Arch.Core;
using SpaceStationGame.Game.Components;

Console.WriteLine("Hello, World!");

var world = World.Create();

// create a resource entity (iron ore) and add it to the world
var iron_ore = world.Create(new RecourseComponent
        {
            ResourceName = "iron_ore",
            WeightKg = 420.23 // kg 
        }
        );

var iron_ingot = world.Create(new RecourseComponent
        {
            ResourceName = "iron_ingot",
            WeightKg = 1.23 // kg 
        }
        );
 



// create a Foundry entity whose recipe produces the iron resource
var foundry = world.Create(
    new InventoryComponent()
    {
        Capacity = 10,
        EntityInventory = [iron_ore,iron_ingot]
    },
    new RecipeComponent()
    {
        Ingredients =[],
        Products = [],
        SpeedMultiplier = 1.0,
        EfficiencyMultiplier = 1.0
    },
    new EnabledComponent()
    {
        IsEnabled = true
    },
    new NameComponent()
    {
        Name_1234 = "Foundry"
    }
);


var eventBus = new EventBus();
eventBus.RegisterChannel(new ConsoleChannel());

var systemScheduler = new SystemScheduler();
systemScheduler.RegisterSystem(new ProductionSystem(world, eventBus), 4);
systemScheduler.RegisterSystem(new PopSystem(eventBus), 2);

var engine = new SimulationEngine(systemScheduler, eventBus);
engine.Run(new());