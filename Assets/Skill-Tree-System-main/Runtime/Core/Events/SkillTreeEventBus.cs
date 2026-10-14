using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.Core.Events
{
    /// <summary>
    /// Default implementation of ISkillTreeEventBus.
    /// Thread-safe, lightweight event bus for skill tree events.
    /// </summary>
    public class SkillTreeEventBus : ISkillTreeEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();
        private readonly object _lock = new object();

        public void Publish<T>(T eventData) where T : SkillTreeEvent
        {
            if (eventData == null)
            {
                Debug.LogWarning("Attempted to publish null event");
                return;
            }

            List<Delegate> handlers;
            lock (_lock)
            {
                if (!_subscribers.TryGetValue(typeof(T), out handlers))
                {
                    return; // No subscribers for this event type
                }

                // Create a copy to avoid issues if handlers modify subscription list
                handlers = new List<Delegate>(handlers);
            }

            // Invoke handlers outside of lock to prevent deadlocks
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking event handler for {typeof(T).Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : SkillTreeEvent
        {
            if (handler == null)
            {
                Debug.LogWarning("Attempted to subscribe with null handler");
                return null;
            }

            lock (_lock)
            {
                var eventType = typeof(T);
                if (!_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _subscribers[eventType] = handlers;
                }

                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                }
            }

            return new Subscription<T>(this, handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : SkillTreeEvent
        {
            if (handler == null) return;

            lock (_lock)
            {
                var eventType = typeof(T);
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);

                    // Clean up empty lists
                    if (handlers.Count == 0)
                    {
                        _subscribers.Remove(eventType);
                    }
                }
            }
        }

        public void ClearAllSubscriptions()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }

        public int GetSubscriberCount<T>() where T : SkillTreeEvent
        {
            lock (_lock)
            {
                if (_subscribers.TryGetValue(typeof(T), out var handlers))
                {
                    return handlers.Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// Disposable subscription handle for automatic unsubscription.
        /// </summary>
        private class Subscription<T> : IDisposable where T : SkillTreeEvent
        {
            private SkillTreeEventBus _eventBus;
            private Action<T> _handler;
            private bool _disposed;

            public Subscription(SkillTreeEventBus eventBus, Action<T> handler)
            {
                _eventBus = eventBus;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed) return;

                _eventBus?.Unsubscribe(_handler);
                _eventBus = null;
                _handler = null;
                _disposed = true;
            }
        }
    }
}
