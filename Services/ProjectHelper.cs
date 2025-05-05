using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using DocCreator01.Contracts;
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

        public Project CreateNewProject()
        {
            var newProject = new Project();
            _currentProject = newProject;
            _currentPath = null;
            
            // Notify listeners that the project has changed
            ProjectChanged?.Invoke(this, _currentProject);
            
            return newProject;
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
                    return false;  // User canceled closing

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
                    return false;  // User canceled save dialog
                }
            }

            // Create a new blank project
            CreateNewProject();
            
            return true;
        }
    }
}
