namespace SpaceStationGame.Engine.Events;

public record EventEnvelope(long Tick, long SimulationTime, int EventTypeId, object EventData);
