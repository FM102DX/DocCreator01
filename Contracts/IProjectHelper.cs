using DocCreator01.Models;
using System;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Helper interface for operations related to Project objects.
    /// </summary>
    public interface IProjectHelper
    {
        /// <summary>
        /// Gets the current active project
        /// </summary>
        Project CurrentProject { get; }

        List<HtmlGenerationProfile> GetHtmlGenerationProfiles();

        /// <summary>
        /// Event that fires when the current project changes
        /// </summary>
        event EventHandler<Project> ProjectChanged;

        /// <summary>
        /// Loads a project from the specified file path
        /// </summary>
        /// <param name="fileName">Path to the project file</param>
        /// <returns>The loaded Project</returns>
        Project LoadProject(string fileName);
        
        /// <summary>
        /// Saves the current project to the specified file path
        /// </summary>
        /// <param name="project">Project to save</param>
        /// <param name="filePath">Path to save the project to</param>
        void SaveProject(Project project, string filePath);

        /// <summary>
        /// Creates a new empty project
        /// </summary>
        /// <returns>A new empty project</returns>
        Project CreateNewProject();

        /// <summary>
        /// Closes the current project and creates a new empty one
        /// </summary>
        /// <param name="saveChanges">Whether to save changes before closing</param>
        /// <returns>True if project was closed, false if operation was canceled</returns>
        bool CloseCurrentProject(bool? saveChanges = null);

    }
}
