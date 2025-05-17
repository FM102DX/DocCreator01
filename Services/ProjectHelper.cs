using System;
using System.IO;
using System.Net.WebSockets;
using System.Windows;
using Microsoft.Win32;
using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;

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
            
            // Notify listeners that the project has changed
            ProjectChanged?.Invoke(this, _currentProject);

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
                    FileName = $"{_currentProject.Name}.docparts"
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
                    HtmlTableCellPaddings = new ElementSpacingInfo { Top = 8, Right = 8, Bottom = 8, Left = 8 },
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
                    HtmlTableCellPaddings = new ElementSpacingInfo { Top = 6, Right = 6, Bottom = 6, Left = 6 },
                    TableHeaderColor = "#F1F3F6",
                    HtmlGenerationPattern = HtmlGenerationPatternEnum.PlainBlueHeader
                }
            };

            return profiles;
        }
    }
}
