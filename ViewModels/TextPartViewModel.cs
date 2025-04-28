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
    }
}
