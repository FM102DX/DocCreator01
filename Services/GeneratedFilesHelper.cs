using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using DocCreator01.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public class GeneratedFilesHelper : IGeneratedFilesHelper
    {
        private readonly IAppPathsHelper _appPathsHelper;
        private readonly IPythonHelper _pythonHelper;
        private readonly IHtmlDocumentCreatorService _htmlDocumentCreator;
        private readonly IBrowserService _browserService;
        private readonly ITextPartHtmlRenderer _textPartHtmlRenderer;

        public GeneratedFilesHelper(
            IAppPathsHelper appPathsHelper,
            IPythonHelper pythonHelper,
            IHtmlDocumentCreatorService htmlDocumentCreator,
            IBrowserService browserService, 
            ITextPartHtmlRenderer textPartHtmlRenderer)
        {
            _appPathsHelper = appPathsHelper ?? throw new ArgumentNullException(nameof(appPathsHelper));
            _pythonHelper = pythonHelper ?? throw new ArgumentNullException(nameof(pythonHelper));
            _htmlDocumentCreator = htmlDocumentCreator ?? throw new ArgumentNullException(nameof(htmlDocumentCreator));
            _browserService = browserService ?? throw new ArgumentNullException(nameof(browserService));
            _textPartHtmlRenderer = textPartHtmlRenderer ?? throw new ArgumentNullException(nameof(textPartHtmlRenderer));
        }

        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        public string OutputDirectory => _appPathsHelper.DocumentsOutputDirectory;

        /// <summary>
        /// Generates a file based on the project and file type
        /// </summary>
        public async Task<string> GenerateFileAsync(Project project, GenerateFileTypeEnum type)
        {
            try
            {
                // Only include text parts marked for inclusion
                var parts = project.ProjectData.TextParts.Where(p => p.IncludeInDocument && !string.IsNullOrEmpty(p.Text)).ToList();
                
                if (!parts.Any())
                {
                    throw new InvalidOperationException("No text parts are selected for inclusion in the document.");
                }

                // Render HTML for all text parts
                _textPartHtmlRenderer.RenderHtml(parts);

                // Generate a filename based on project name
                string fileName = $"{project.Name}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.{type.ToString().ToLower()}";
                string filePath = string.Empty;
                
                // Branch based on document type
                switch (type)
                {
                    case GenerateFileTypeEnum.DOCX:
                        filePath = await _pythonHelper.CreateDocumentAsync(type, parts, fileName);
                        break;
                        
                    case GenerateFileTypeEnum.HTML:
                        filePath = await _htmlDocumentCreator.CreateDocumentAsync(parts, fileName);
                        break;
                        
                    default:
                        throw new NotSupportedException($"Document type {type} is not supported.");
                }

                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during document generation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Opens a generated file using the appropriate application
        /// </summary>
        public bool OpenFile(GeneratedFile generatedFile)
        {
            if (generatedFile == null || !generatedFile.Exists)
                return false;

            switch (generatedFile.FileType)
            {
                case GenerateFileTypeEnum.HTML:
                    return _browserService.OpenInNotepadPlusPlus(generatedFile.FilePath);
                    
                case GenerateFileTypeEnum.DOCX:
                    // Open using default application for DOCX
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = generatedFile.FilePath,
                            UseShellExecute = true
                        });
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                    
                default:
                    return false;
            }
        }

        /// <summary>
        /// Deletes a generated file from disk and removes it from the project
        /// </summary>
        public bool DeleteFile(GeneratedFile generatedFile, ObservableCollection<GeneratedFile> generatedFiles)
        {
            if (generatedFile == null)
                return false;

            try
            {
                // Delete file from disk if it exists
                if (File.Exists(generatedFile.FilePath))
                {
                    File.Delete(generatedFile.FilePath);
                }

                // Remove from collection
                generatedFiles.Remove(generatedFile);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refreshes the list of generated file view models
        /// </summary>
        public void RefreshGeneratedFileViewModels(ObservableCollection<GeneratedFile> generatedFiles, ObservableCollection<GeneratedFileViewModel> generatedFileViewModels)
        {
            generatedFileViewModels.Clear();
            
            // Create view models for existing files
            foreach (var generatedFile in GetExistingFiles(generatedFiles))
            {
                generatedFileViewModels.Add(new GeneratedFileViewModel(generatedFile, _appPathsHelper));
            }
        }

        /// <summary>
        /// Gets all generated files that exist on disk
        /// </summary>
        public List<GeneratedFile> GetExistingFiles(ObservableCollection<GeneratedFile> generatedFiles)
        {
            List<GeneratedFile> existingFiles = new List<GeneratedFile>();
            
            foreach (var generatedFile in generatedFiles)
            {
                if (generatedFile.Exists)
                {
                    existingFiles.Add(generatedFile);
                }
            }
            
            return existingFiles;
        }
    }
}
