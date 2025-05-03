using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.ViewModels
{
    public sealed class TextPartViewModel : ReactiveObject
    {
        public TextPart Model { get; }

        public TextPartViewModel(TextPart model) => Model = model;

        public string Text
        {
            get => Model.Text;
            set
            {
                if (value == Model.Text) return;
                Model.Text = value;
                this.RaisePropertyChanged();
            }
        }

        public int Level
        {
            get => Model.Level;
            set
            {
                if (value == Model.Level) return;
                Model.Level = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(DisplayTitle));
            }
        }

        // Property to display the title with proper indentation based on level
        public string DisplayTitle => new string(' ', (Level - 1) * 3) + Model.Title;
    }
}
