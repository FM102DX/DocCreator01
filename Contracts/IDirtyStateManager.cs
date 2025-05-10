using System;
using System.Collections.Generic;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Manager for tracking dirty state across a hierarchy of objects
    /// </summary>
    public interface IDirtyStateManager
    {
        /// <summary>
        /// Gets a value indicating whether this object or any of its subscriptions is dirty
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Occurs when the dirty state changes
        /// </summary>
        event Action IBecameDirty;
        event Action DirtryStateWasReset;

        /// <summary>
        /// Accepts all changes and resets the dirty state to false
        /// </summary>
        void ResetDirtyState();

        /// <summary>
        /// Marks this object as dirty
        /// </summary>
        void MarkAsDirty();

        /// <summary>
        /// Adds a subscription to another dirty trackable object
        /// </summary>
        /// <param name="trackable">The object to track</param>
        void AddSubscription(IDirtyTrackable trackable);


    }
}
