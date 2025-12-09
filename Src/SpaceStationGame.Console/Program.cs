// See https://aka.ms/new-console-template for more information
using SpaceStationGame.Engine;
using SpaceStationGame.Engine.Events;
using SpaceStationGame.Engine.Systems;
using SpaceStationGame.Game;
using SpaceStationGame.Game.Channels;

Console.WriteLine("Hello, World!");

var eventBus = new EventBus();

eventBus.RegisterChannel(new ConsoleChannel());

var systemScheduler = new SystemScheduler();

systemScheduler.RegisterSystem(new ProductionSystem(), 4);
systemScheduler.RegisterSystem(new PopSystem(eventBus), 2);

var engine = new SimulationEngine(systemScheduler, eventBus);

engine.Run(new());