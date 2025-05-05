using DocCreator01.Data.Enums;
using DocCreator01.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Interface for managing generated files operations
    /// </summary>
    public interface IGeneratedFilesHelper
    {
        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        string OutputDirectory { get; }

        /// <summary>
        /// Generates a file based on the project and file type
        /// </summary>
        /// <param name="project">The project containing text parts to generate file from</param>
        /// <param name="type">The type of file to generate</param>
        /// <returns>Path to the generated file</returns>
        Task<string> GenerateFileAsync(Project project, GenerateFileTypeEnum type);

        /// <summary>
        /// Opens a generated file using the appropriate application
        /// </summary>
        /// <param name="generatedFile">The file to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        bool OpenFile(GeneratedFile generatedFile);

        /// <summary>
        /// Deletes a generated file from disk and removes it from the project
        /// </summary>
        /// <param name="generatedFile">The file to delete</param>
        /// <param name="generatedFiles">Collection of generated files to update</param>
        /// <returns>True if successfully deleted, false otherwise</returns>
        bool DeleteFile(GeneratedFile generatedFile, ObservableCollection<GeneratedFile> generatedFiles);

        /// <summary>
        /// Refreshes the list of generated file view models
        /// </summary>
        /// <param name="generatedFiles">Collection of generated file models</param>
        /// <param name="generatedFileViewModels">Collection of view models to update</param>
        void RefreshGeneratedFileViewModels(ObservableCollection<GeneratedFile> generatedFiles, ObservableCollection<GeneratedFileViewModel> generatedFileViewModels);

        /// <summary>
        /// Gets all generated files that exist on disk
        /// </summary>
        /// <param name="generatedFiles">Collection of generated files to check</param>
        /// <returns>List of existing generated files</returns>
        List<GeneratedFile> GetExistingFiles(ObservableCollection<GeneratedFile> generatedFiles);
    }
}
