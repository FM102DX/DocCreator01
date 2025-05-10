using ReactiveUI;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Interface for all tab view models to implement common functionality
    /// </summary>
    public interface ITabViewModel
    {
        /// <summary>
        /// Gets the header text for the tab
        /// </summary>
        string TabHeader { get; }
        
        /// <summary>
        /// Indicates whether the tab has unsaved changes
        /// </summary>
        bool IsDirty { get; }
        
        /// <summary>
        /// Called after the project has been successfully saved to reset dirty state
        /// </summary>
        void AcceptChanges();
        
        /// <summary>
        /// Manually marks the tab as dirty
        /// </summary>
        void MarkAsDirty();
    }
}
