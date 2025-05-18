using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DocCreator01.ViewModels
{
    public sealed class MainGridItemViewModel : ReactiveObject
    {
        private readonly TextPart _textPart;
        public MainGridItemViewModel(TextPart textPart)
        {
            _textPart = textPart;
            this.WhenAnyValue(x => x._textPart.Level)
                .Subscribe(_ => 
                {
                    this.RaisePropertyChanged(nameof(Indentation));
                    this.RaisePropertyChanged(nameof(Level));
                });
            this.WhenAnyValue(x => x._textPart.Name)
                .Subscribe(_ => 
                {
                    this.RaisePropertyChanged(nameof(Name));
                });
        }

        public TextPart Model => _textPart;
        public string Name => _textPart.Name;
        public int Level => _textPart.Level;
        public string ParagraphNo => _textPart.ParagraphNo;
        public string Indentation
        {
            get
            {
                if (_textPart.Level <= 1)
                    return string.Empty;
                return new string('-', (_textPart.Level - 1) * 2);
            }
        }
    }
}
