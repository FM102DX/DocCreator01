using DocCreator01.Models;
using System;
using System.Collections.Generic;

namespace DocCreator01.Contracts
{
    public interface IProjectHelper
    {
        Project CurrentProject { get; }

        List<HtmlGenerationProfile> GetHtmlGenerationProfiles();

        event EventHandler<Project> ProjectChanged;

        Project LoadProject(string fileName);

        void SaveProject(Project project, string filePath);

        void CreateNewProject();

        bool CloseCurrentProject(bool? saveChanges = null);

        /// <summary>
        /// Ensures that each TextPart has the first chunk filled with the Text
        /// (used right after loading a project from file).
        /// </summary>
        /// <param name="project">Project to inspect / fix.</param>
        void EnsureTextPartChunks(Project project);
    }
}