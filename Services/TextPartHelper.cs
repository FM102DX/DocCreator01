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

        public bool MoveTextPartUp(TextPart textPart, ObservableCollection<TextPart> textParts, ObservableCollection<MainGridItemViewModel> viewModels)
        {
            int idx = textParts.IndexOf(textPart);
            if (idx > 0)
            {
                textParts.Move(idx, idx - 1);
                return true;
            }
            return false;
        }

        public bool MoveTextPartDown(TextPart textPart, ObservableCollection<TextPart> textParts, ObservableCollection<MainGridItemViewModel> viewModels)
        {
            int idx = textParts.IndexOf(textPart);
            if (idx < textParts.Count - 1 && idx >= 0)
            {
                textParts.Move(idx, idx + 1);
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
