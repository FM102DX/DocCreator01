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

        Project CreateNewProject();

        bool CloseCurrentProject(bool? saveChanges = null);
    }
}