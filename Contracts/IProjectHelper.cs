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
        
        bool SaveProject(Project project, List<Guid>? openedTabs = null);

        bool SaveProjectAs();

        void CreateNewProject();

        bool CloseCurrentProject(bool? saveChanges = null);

        void EnsureTextPartChunks(Project project);

        bool RemoveTextPartChunk(TextPart textPart, TextPartChunk chunk);

        TextPartChunk AddEmptyChunkIfNeeded(TextPart textPart);
        
        TextPartChunk AddEmptyChunk(TextPart textPart);
    }
}