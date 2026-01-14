using System;
using System.Collections.Generic;

public sealed class EventBus
{
    private static readonly EventBus _instance = new EventBus();
    public static EventBus Instance { get { return _instance; } }

    private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

    private EventBus() { }

    public void Register<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _handlers[type] = list;
        }

        if (!list.Contains(handler))
        {
            list.Add(handler);
        }
    }

    public void Unregister<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list))
            return;

        list.Remove(handler);
        if (list.Count == 0)
            _handlers.Remove(type);
    }

    public void Publish<T>(T evnt) where T : IEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list))
            return;

        // Copy to avoid modification during enumeration
        var snapshot = list.ToArray();
        foreach (var del in snapshot)
        {
            if (del is Action<T> action)
                action(evnt);
        }
    }
}