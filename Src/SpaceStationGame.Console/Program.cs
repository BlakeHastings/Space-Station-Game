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


var iron_ore_type = world.Create(new RecourseTypeComponent
 {
        RecourseName = "iron_ore",
        meltingPointCelsius = 1560.0,
        stackability = Entity.Null // placeholder
    }
    );
var iron_ingot_type = world.Create(new RecourseTypeComponent
 {
        RecourseName = "iron_ingot",
        meltingPointCelsius = 1538.0,
        stackability = Entity.Null // placeholder
    }
    );

 
var copper_ingot_type = world.Create(new RecourseTypeComponent
 {
        RecourseName = "copper_ingot",
        meltingPointCelsius = 1085.0,
        stackability = Entity.Null // placeholder
    }
    );


// create a resource entity (iron ore) and add it to the world
var iron_ore = world.Create(new RecourseComponent
        {
            RecourseTypeComponent = iron_ore_type,
            WeightKg = 20.23 // kg 
        }
        );

var iron_ingot = world.Create(new RecourseComponent
        {
            RecourseTypeComponent = iron_ingot_type,
            WeightKg = 0.0 // kg 
        }
        );
 
var iron_ingot_recipe = world.Create(new RecourseComponent
        {
            RecourseTypeComponent = iron_ingot_type,
            WeightKg = 0.2 // kg 
        }
        );
var iron_ore_recipe = world.Create(new RecourseComponent
        {
           RecourseTypeComponent = iron_ore_type,
            WeightKg = 0.8 // kg 
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
        
        Ingredients =[iron_ore_recipe],
        Products = [iron_ingot_recipe],
        SpeedMultiplier = 1.0,
        EfficiencyMultiplier = 1.3
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