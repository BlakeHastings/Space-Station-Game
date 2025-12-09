namespace SpaceStationGame.Engine.Events;

public class EventTypeRegistry
{
    private readonly Dictionary<int, Type> _idToType = new();
    private readonly Dictionary<Type, int> _typeToId = new();

    public void Register<T>() where T : IEventData
    {
        var id = T.EventTypeId;
        _idToType[id] = typeof(T);
        _typeToId[typeof(T)] = id;
    }

    public int GetId<T>() where T : IEventData => T.EventTypeId;

    public int GetId(Type type)
    {
        if (_typeToId.TryGetValue(type, out var id))
            return id;
        throw new InvalidOperationException($"Event type {type.Name} is not registered");
    }

    public Type GetType(int id)
    {
        if (_idToType.TryGetValue(id, out var type))
            return type;
        throw new InvalidOperationException($"Event type ID {id} is not registered");
    }
}
