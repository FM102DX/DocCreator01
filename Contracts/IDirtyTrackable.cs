using System;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Interface for objects that can track their dirty state through a manager
    /// </summary>
    public interface IDirtyTrackable
    {
        /// <summary>
        /// Gets the dirty state manager for this object
        /// </summary>
        IDirtyStateManager DirtyStateMgr { get; }
    }
}
