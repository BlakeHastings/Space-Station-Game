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

world.Create(new InventoryComponent()
{
    Capacity = 10,
    EntityInventory = []
}, new RecipeComponent()
{
    Ingredients = [],
    Products = [],
    SpeedMultiplier = 1.0,
    EfficiencyMultiplier = 1.0
},
new EnabledComponent()
{
    IsEnabled = true
});


var eventBus = new EventBus();
eventBus.RegisterChannel(new ConsoleChannel());

var systemScheduler = new SystemScheduler();
systemScheduler.RegisterSystem(new ProductionSystem(world, eventBus), 4);
systemScheduler.RegisterSystem(new PopSystem(eventBus), 2);

var engine = new SimulationEngine(systemScheduler, eventBus);
engine.Run(new());