using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DocCreator01.Models;
using ReactiveUI;

namespace DocCreator01.ViewModels
{
    public sealed class TabPageViewModel : ReactiveObject
    {
        private TextPart? textPart;

        public TabPageViewModel(TextPart? textPart)
        {
            this.textPart = textPart;
        }

        public TextPart? TextPart
        {
            get => textPart;
            set => this.RaiseAndSetIfChanged(ref textPart, value);
        }
        public string? Title => TextPart?.Title;
    }
}
