using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using DocCreator01.Utils;
using DocCreator01.ViewModels;
using DynamicData;
using ReactiveUI;
using DocCreator01.Messages;
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
        private Project _project;
        public ObservableCollection<GeneratedFile> GeneratedFiles { get; set; } = new();

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

        public void Initialize(Project project)
        {
            _project = project;
            if(_project!=null)
                _project.ProjectData.GeneratedFiles = GetExistingFiles();
        }
        public string OutputDirectory => _appPathsHelper.DocumentsOutputDirectory;

        public async Task GenerateFileAsync(GenerateFileTypeEnum type)
        {
            if (_project == null) return;
            try
            {
                // Only include text parts marked for inclusion
                var contentParts = _project.ProjectData.TextParts
                                      .Where(p => p.IncludeInDocument)
                                      .ToList();

                if (!contentParts.Any())
                    throw new InvalidOperationException("No text parts are selected for inclusion in the document.");

                //--------------------------------------------------
                // 1. Build header table (no caption) from settings
                //--------------------------------------------------
                var s = _project.Settings;
                string headerTableHtml = $@"
<table style=""width:100%;border-collapse:collapse;margin-bottom:30px;"">
    <tr><td style=""font-weight:600;padding:4px 8px;"">Title</td><td style=""padding:4px 8px;"">{System.Net.WebUtility.HtmlEncode(s.DocTitle)}</td></tr>
    <tr><td style=""font-weight:600;padding:4px 8px;"">Description</td><td style=""padding:4px 8px;"">{System.Net.WebUtility.HtmlEncode(s.DocDescription)}</td></tr>
    <tr><td style=""font-weight:600;padding:4px 8px;"">Created By</td><td style=""padding:4px 8px;"">{System.Net.WebUtility.HtmlEncode(s.DocCretaedBy)}</td></tr>
</table>";

                var headerPart = new TextPart
                {
                    Name      = string.Empty,
                    Text      = headerTableHtml,   // keep same for completeness
                    Html      = headerTableHtml,   // already-prepared HTML
                    Level     = 1,
                    IncludeInDocument = true
                };

                //--------------------------------------------------
                // 2. Render HTML for all *content* parts
                //--------------------------------------------------
                _textPartHtmlRenderer.RenderHtml(contentParts);

                // 3. Merge header + content
                var allParts = new List<TextPart> { headerPart };
                allParts.AddRange(contentParts);

                //--------------------------------------------------
                // 4. Generate output file
                //--------------------------------------------------
                string fileName = $"{_project.Name}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.{type.ToString().ToLower()}";
                string filePath;

                switch (type)
                {
                    case GenerateFileTypeEnum.DOCX:
                        filePath = await _pythonHelper.CreateDocumentAsync(type, allParts, fileName);
                        break;
                    case GenerateFileTypeEnum.HTML:
                        filePath = _htmlDocumentCreator.CreateDocument(
                                       allParts, 
                                       fileName,
                                       _project.Settings.CurrentHtmlGenerationProfile);
                        break;
                    default:
                        throw new NotSupportedException($"Document type {type} is not supported.");
                }

                _project.ProjectData.GeneratedFiles.Add(new GeneratedFile
                {
                    FilePath = filePath, 
                    FileType = type, 
                    Project  = _project
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during document generation: {ex.Message}");
                throw;
            }
        }

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

        public bool DeleteFile(GeneratedFile generatedFile, ObservableCollection<GeneratedFile> generatedFiles)
        {
            if (generatedFile == null)
                return false;

            try
            {
                if (File.Exists(generatedFile.FilePath))
                {
                    File.Delete(generatedFile.FilePath);
                }
                generatedFiles.Remove(generatedFile);
                RefreshExistingFiles();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }

        public bool RenameFile(GeneratedFile generatedFile, string newName)
        {
            if (generatedFile == null || string.IsNullOrWhiteSpace(newName))
                return false;

            try
            {
                // Validate the new filename
                var (isValid, _) = FileNameValidator.Validate(newName);
                if (!isValid)
                {
                    return false;
                }
                
                // Get directory and original file extension
                string directory = Path.GetDirectoryName(generatedFile.FilePath);
                string extension = Path.GetExtension(generatedFile.FilePath);
                
                // Ensure the new name has the proper extension
                if (!newName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    newName += extension;
                
                // Create new full path
                string newPath = Path.Combine(directory, newName);
                
                // Check if file exists
                if (File.Exists(newPath) && !string.Equals(generatedFile.FilePath, newPath, StringComparison.OrdinalIgnoreCase))
                    return false;  // Don't overwrite existing file
                
                // Rename the file on disk
                if (File.Exists(generatedFile.FilePath))
                {
                    // Don't try to rename if it would be the same file
                    if (string.Equals(generatedFile.FilePath, newPath, StringComparison.OrdinalIgnoreCase))
                        return true;

                    File.Move(generatedFile.FilePath, newPath);
                    
                    // Update the model
                    generatedFile.FilePath = newPath;
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error renaming file: {ex.Message}");
                return false;
            }
        }

        public void RefreshExistingFiles()
        {
            if (_project == null || _project.ProjectData == null) return;
            _project.ProjectData.GeneratedFiles = GetExistingFiles();
            MessageBus.Current.SendMessage(new GeneratedFilesUpdatedMessage());
        }
         
        public ObservableCollection<GeneratedFile> GetExistingFiles()
        {
            ObservableCollection<GeneratedFile> existingFiles = new ObservableCollection<GeneratedFile>();
            if (_project == null || _project.ProjectData == null || _project.ProjectData.GeneratedFiles == null)
                return existingFiles;

            foreach (var generatedFile in _project.ProjectData.GeneratedFiles)
            {
                if (generatedFile.Exists)
                {
                    generatedFile.Project = _project;
                    existingFiles.Add(generatedFile);
                }
            }
            return existingFiles;
        }

        public void DeleteAllFiles()
        {
            if (_project == null || _project.ProjectData == null || _project.ProjectData.GeneratedFiles == null)
                return;

            // Create a copy of the list to iterate over, as we'll be modifying the original
            var filesToDelete = new List<GeneratedFile>(_project.ProjectData.GeneratedFiles);

            foreach (var fileModel in filesToDelete)
            {
                try
                {
                    if (File.Exists(fileModel.FilePath))
                    {
                        File.Delete(fileModel.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle error for individual file deletion
                    System.Diagnostics.Debug.WriteLine($"Error deleting file {fileModel.FilePath}: {ex.Message}");
                }
            }

            _project.ProjectData.GeneratedFiles.Clear();
            RefreshExistingFiles(); // This will send the message to update UI
        }
    }
}
