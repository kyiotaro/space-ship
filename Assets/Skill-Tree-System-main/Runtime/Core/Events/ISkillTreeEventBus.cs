using System;

namespace SkillTree.Core.Events
{
    /// <summary>
    /// Event bus for skill tree events.
    /// Provides decoupled communication between components.
    /// </summary>
    public interface ISkillTreeEventBus
    {
        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        void Publish<T>(T eventData) where T : SkillTreeEvent;

        /// <summary>
        /// Subscribes to a specific event type.
        /// Returns an IDisposable that can be used to unsubscribe.
        /// </summary>
        IDisposable Subscribe<T>(Action<T> handler) where T : SkillTreeEvent;

        /// <summary>
        /// Unsubscribes a handler from a specific event type.
        /// </summary>
        void Unsubscribe<T>(Action<T> handler) where T : SkillTreeEvent;

        /// <summary>
        /// Clears all subscribers for all event types.
        /// </summary>
        void ClearAllSubscriptions();

        /// <summary>
        /// Gets the number of subscribers for a specific event type.
        /// </summary>
        int GetSubscriberCount<T>() where T : SkillTreeEvent;
    }
}
