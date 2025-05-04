using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DocCreator01.Contracts;
using DocCreator01.Models;
using DocCreator01.ViewModels;

namespace DocCreator01.Services
{
    public class TextPartHelper : ITextPartHelper
    {
        private readonly IProjectRepository _repo;

        public TextPartHelper(IProjectRepository repo)
        {
            _repo = repo;
        }

        public TextPart CreateTextPart(Project project)
        {
            return new TextPart
            {
                Id = Guid.NewGuid(),
                Title = project.GetNewTextPartName(),
                Text = $"Tab {project.ProjectData.TextParts.Count + 1}",
                Name = $"Part {project.ProjectData.TextParts.Count + 1}",
                IncludeInDocument = true
            };
        }

        public Project LoadProject(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Project file not found: {fileName}");
            }

            var project = _repo.Load(fileName);
            project.FilePath = fileName;
            
            return project;
        }

        public void SaveProject(Project project, string filePath)
        {
            _repo.Save(project, filePath);
            project.FilePath = filePath; // Ensure FilePath is always updated
        }

        public void RefreshTextPartViewModels(ObservableCollection<TextPart> textParts, ObservableCollection<MainGridItemViewModel> viewModels)
        {
            viewModels.Clear();

            foreach (var textPart in textParts)
            {
                viewModels.Add(new MainGridItemViewModel(textPart));
            }

            // Ensure we stay synchronized with the model collection
            textParts.CollectionChanged += (s, e) =>
            {
                // Re-build the view models collection when the underlying collection changes
                viewModels.Clear();
                foreach (var textPart in textParts)
                {
                    viewModels.Add(new MainGridItemViewModel(textPart));
                }
            };
        }

        public bool MoveTextPartUp(TextPart textPart, ObservableCollection<TextPart> textParts, ObservableCollection<MainGridItemViewModel> viewModels)
        {
            int idx = textParts.IndexOf(textPart);

            if (idx > 0)
            {
                // Move in the model collection
                textParts.Move(idx, idx - 1);

                // Move in the view model collection
                viewModels.Move(idx, idx - 1);
                
                return true;
            }

            return false;
        }

        public bool MoveTextPartDown(TextPart textPart, ObservableCollection<TextPart> textParts, ObservableCollection<MainGridItemViewModel> viewModels)
        {
            int idx = textParts.IndexOf(textPart);

            if (idx < textParts.Count - 1 && idx >= 0)
            {
                // Move in the model collection
                textParts.Move(idx, idx + 1);

                // Move in the view model collection
                viewModels.Move(idx, idx + 1);
                
                return true;
            }

            return false;
        }

        public void RemoveTextPart(TextPart textPart, ObservableCollection<TextPart> textParts, ObservableCollection<MainGridItemViewModel> viewModels)
        {
            var viewModel = viewModels.FirstOrDefault(vm => vm.Model == textPart);
            
            if (viewModel != null)
            {
                viewModels.Remove(viewModel);
            }

            textParts.Remove(textPart);
        }
        
        public bool DecreaseTextPartLevel(TextPart textPart)
        {
            if (textPart.Level > 1)
            {
                textPart.Level--;
                // The view model will update automatically via reactive binding
                return true;
            }
            return false;
        }
        
        public bool IncreaseTextPartLevel(TextPart textPart)
        {
            if (textPart.Level < 5)
            {
                textPart.Level++;
                // The view model will update automatically via reactive binding
                return true;
            }
            return false;
        }
    }
}
