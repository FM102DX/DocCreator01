using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DocCreator01.Contracts;
using DocCreator01.Models;
using DocCreator01.ViewModels;

namespace DocCreator01.Services
{
    public class TextPartHelper : ITextPartHelper
    {
        // Constructor no longer needs repository
        public TextPartHelper()
        {
        }

        public TextPart CreateTextPart(Project project)
        {
            return new TextPart
            {
                Id = Guid.NewGuid(),
                Text = $"Tab {project.ProjectData.TextParts.Count + 1}",
                Name = project.GetNewTextPartName(),
                IncludeInDocument = true
            };
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

                // Refresh view model collection to ensure UI is updated
                RefreshTextPartViewModels(textParts, viewModels);
                
                // Find the moved item in the refreshed view models and return it
                // so the calling code can restore the selection
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

                // Refresh view model collection to ensure UI is updated
                RefreshTextPartViewModels(textParts, viewModels);
                
                // Find the moved item in the refreshed view models and return it
                // so the calling code can restore the selection
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
