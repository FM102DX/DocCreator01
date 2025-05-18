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
    }
}
