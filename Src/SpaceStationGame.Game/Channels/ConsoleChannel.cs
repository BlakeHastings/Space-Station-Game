using System.Text.Json;
using SpaceStationGame.Engine.Events;

namespace SpaceStationGame.Game.Channels;

public class ConsoleChannel : IEventChannel
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    public void ReceiveBatch(List<EventEnvelope> events)
    {
        foreach (var envelope in events)
        {
            Console.WriteLine(JsonSerializer.Serialize(envelope, envelope.GetType(), SerializerOptions));
        }
    }
}
