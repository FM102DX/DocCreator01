using DocCreator01.Data.Enums;
using DocCreator01.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DocCreator01.ViewModels;

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
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        void Initialize(Project project);

        /// <summary>
        /// Generates a file based on the project and file type
        /// </summary>
        /// <param name="type">The type of file to generate</param>
        /// <returns>Path to the generated file</returns>
        Task GenerateFileAsync(GenerateFileTypeEnum type);

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
        bool DeleteFile(GeneratedFile generatedFile, List<GeneratedFile> generatedFiles);

        /// <summary>
        /// Renames a generated file
        /// </summary>
        /// <param name="generatedFile">The file to rename</param>
        /// <param name="newName">New file name (without path)</param>
        /// <returns>True if successfully renamed, false otherwise</returns>
        bool RenameFile(GeneratedFile generatedFile, string newName);

        void DeleteAllFiles();
        void RefreshExistingFiles();
    }
}
