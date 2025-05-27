using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.WebSockets;
using System.Windows;
using Microsoft.Win32;
using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using DocCreator01.Messages;
using System.Linq;
using DocCreator01.Services;  // allow calling TextPartHelper

namespace DocCreator01.Services
{
    public class ProjectHelper : IProjectHelper
    {
        private readonly IProjectRepository _repo;
        private Project _currentProject = new();
        private string? _currentPath;

        public ProjectHelper(IProjectRepository repo)
        {
            _repo = repo;
        }

        public Project CurrentProject => _currentProject;

        public event EventHandler<Project> ProjectChanged;

        public Project LoadProject(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Project file not found: {fileName}");
            }
            var project = _repo.Load(fileName);
            project.FilePath = fileName;
            
            // Update current project and path
            _currentProject = project;
            _currentPath = fileName;
            project.Settings.CurrentHtmlGenerationProfile = 
                GetHtmlGenerationProfiles().FirstOrDefault(p=>p.Id== project.Settings.CurrentHtmlGenerationProfileId);
            
            EnsureTextPartChunks(project);          // <-- new fix-up

            // Notify listeners that the project has changed
            ProjectChanged?.Invoke(this, _currentProject);

            // Notify the application that a project was loaded
            MessageBus.Current.SendMessage(new ProjectLoadedMessage(project));
            
            return project;
        }

        public void SaveProject(Project project, string filePath)
        {
            _repo.Save(project, filePath);
            project.FilePath = filePath; // Ensure FilePath is always updated
            _currentPath = filePath;
        }

        public void CreateNewProject()
        {
            _currentProject = new Project();
            ProjectChanged?.Invoke(this, _currentProject);
        }

        public bool CloseCurrentProject(bool? saveChanges = null)
        {
            // If saveChanges is provided, respect it. Otherwise, ask the user.
            if (saveChanges == null)
            {
                var res = MessageBox.Show(
                    "Сохранить изменения перед закрытием?",
                    "Закрыть документ",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel)
                    return false; // User canceled closing

                if (res == MessageBoxResult.Yes)
                    saveChanges = true;
                else
                    saveChanges = false;
            }

            // Save if requested
            if (saveChanges == true && !string.IsNullOrEmpty(_currentPath))
            {
                SaveProject(_currentProject, _currentPath);
            }
            else if (saveChanges == true)
            {
                // Need to get a filename
                var dlg = new SaveFileDialog
                {
                    Filter = "Doc Parts (*.docparts)|*.docparts",
                    DefaultExt = ".docparts",
                    FileName = "Свой_конфлис_на_сищарп.docparts"
                };

                if (dlg.ShowDialog() == true)
                {
                    SaveProject(_currentProject, dlg.FileName);
                }
                else
                {
                    return false; // User canceled save dialog
                }
            }

            // Create a new blank project
            CreateNewProject();

            return true;
        }

        public bool SaveProjectAs()
        {
            // First save the current project if it has a path
            if (!string.IsNullOrEmpty(_currentPath))
            {
                SaveProject(_currentProject, _currentPath);
            }
            
            // Show save dialog
            var dlg = new SaveFileDialog
            {
                Filter = "Doc Parts (*.docparts)|*.docparts",
                DefaultExt = ".docparts",
                FileName = $"Свой_конфлис_на_сищарп.docparts" // Set default filename as shown in the screenshot
            };
            
            if (dlg.ShowDialog() != true)
            {
                return false; // User canceled
            }
            
            // Get new path
            var newPath = dlg.FileName;
            
            // Skip if same path
            if (newPath == _currentPath)
            {
                return true;
            }
            
            // Check if file exists and confirm overwrite
            if (File.Exists(newPath))
            {
                var result = MessageBox.Show("Файл уже существует. Перезаписать?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return false; // User chose not to overwrite
                }
            }
            
            // Save the current project to the new location
            SaveProject(_currentProject, newPath);
            _currentProject.FilePath = newPath;
            
            return true;
        }
        
        public List<HtmlGenerationProfile> GetHtmlGenerationProfiles()
        {
            var profiles = new List<HtmlGenerationProfile>
            {
                new HtmlGenerationProfile
                {
                    Id = 1,
                    Name = "AsGpt",
                    HtmlFontSize = 14,
                    HtmlH1Margins = new ElementSpacingInfo { Top = 35, Right = 0, Bottom = 16, Left = 0 },
                    HtmlH2Margins = new ElementSpacingInfo { Top = 30, Right = 0, Bottom = 14, Left = 0 },
                    HtmlH3Margins = new ElementSpacingInfo { Top = 25, Right = 0, Bottom = 12, Left = 0 },
                    HtmlH4Margins = new ElementSpacingInfo { Top = 14, Right = 0, Bottom = 10, Left = 0 },
                    HtmlH5Margins = new ElementSpacingInfo { Top = 12, Right = 0, Bottom = 8, Left = 0 },
                    HtmlDocumentPaddings = new ElementSpacingInfo { Top = 100, Right = 100, Bottom = 100, Left = 100 },
                    HtmlTableCellPaddings = new ElementSpacingInfo { Top = 10, Right = 12, Bottom = 5, Left = 3 },
                    TableHeaderColor = "#F1F3F6",
                    HtmlGenerationPattern = HtmlGenerationPatternEnum.AsChatGpt
                },
                new HtmlGenerationProfile
                {
                    Id = 2,
                    Name = "TablesBlueHeaders",
                    HtmlFontSize = 14,
                    HtmlH1Margins = new ElementSpacingInfo { Top = 20, Right = 0, Bottom = 12, Left = 0 },
                    HtmlH2Margins = new ElementSpacingInfo { Top = 16, Right = 0, Bottom = 10, Left = 0 },
                    HtmlH3Margins = new ElementSpacingInfo { Top = 14, Right = 0, Bottom = 8, Left = 0 },
                    HtmlH4Margins = new ElementSpacingInfo { Top = 12, Right = 0, Bottom = 6, Left = 0 },
                    HtmlH5Margins = new ElementSpacingInfo { Top = 10, Right = 0, Bottom = 4, Left = 0 },
                    HtmlTableCellPaddings = new ElementSpacingInfo { Top = 10, Right = 12, Bottom = 5, Left = 3 },
                    TableHeaderColor = "#F1F3F6",
                    HtmlGenerationPattern = HtmlGenerationPatternEnum.PlainBlueHeader
                }
            };

            return profiles;
        }

        // NEW: make sure chunks collection contains the main text
        public void EnsureTextPartChunks(Project project)
        {
            if (project?.ProjectData?.TextParts == null) return;

            foreach (var tp in project.ProjectData.TextParts)
            {
                if (string.IsNullOrWhiteSpace(tp?.Text)) continue;

                // ensure collection exists
                if(tp.TextPartChunks == null)
                    tp.TextPartChunks =new  List<TextPartChunk>();

                // if first chunk missing or empty – create / fill it
                if (tp.TextPartChunks.Count == 0 ||
                    string.IsNullOrWhiteSpace(tp.TextPartChunks[0]?.Text))
                {
                    var chunk = new TextPartChunk
                    {
                        Id   = Guid.NewGuid(),
                        Text = tp.Text
                    };

                    if (tp.TextPartChunks.Count == 0)
                        tp.TextPartChunks.Add(chunk);
                    else
                        tp.TextPartChunks[0] = chunk;
                }
            }
        }
        
        // Simplified: Just handle basic chunk removal without the empty chunk logic
        public bool RemoveTextPartChunk(TextPart textPart, TextPartChunk chunk)
        {
            if (textPart == null || chunk == null) return false;
            
            // Remove the chunk from the TextPart and return success/failure
            if (textPart.TextPartChunks.Contains(chunk))
            {
                textPart.TextPartChunks.Remove(chunk);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Adds an empty chunk to the TextPart if needed (typically at the end of the collection)
        /// </summary>
        /// <param name="textPart">TextPart to add chunk to</param>
        /// <returns>The newly created chunk if one was added, otherwise null</returns>
        public TextPartChunk AddEmptyChunkIfNeeded(TextPart textPart)
            => TextPartHelper.AddEmptyChunkIfNeeded(textPart);

        /// <summary>
        /// Force adds a new empty chunk to a TextPart
        /// </summary>
        /// <param name="textPart">TextPart to add chunk to</param>
        /// <returns>The newly created chunk</returns>
        public TextPartChunk AddEmptyChunk(TextPart textPart)
            => TextPartHelper.AddEmptyChunk(textPart);
    }
}
