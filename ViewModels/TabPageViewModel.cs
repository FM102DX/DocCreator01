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
        

        bool _isDirty;
        private TextPart? _textPart;
        public string? Title => TextPart?.Title;
        public bool IsDirty
        {
            get => _isDirty;
            private set => this.RaiseAndSetIfChanged(ref _isDirty, value);
        }

        // заголовок для вкладки (с * если IsDirty)
        public string Header => IsDirty ? $"{TextPart.Title} *" : TextPart.Title;

        public TabPageViewModel(TextPart? textPart)
        {
            this._textPart = textPart;
            this.WhenAnyValue(_ => _.TextPart.Title,
                    _ => _.TextPart.Text)
                .Skip(1)
                .Subscribe(_ =>
                {
                    IsDirty = true;                      // ← вот здесь флаг «грязно»
                    this.RaisePropertyChanged(nameof(Header));
                });
        }

        public TextPart? TextPart
        {
            get => _textPart;
            set => this.RaiseAndSetIfChanged(ref _textPart, value);
        }
        /// <summary>Вызывается после успешного сохранения проекта.</summary>
        public void AcceptChanges()
        {
            IsDirty = false;
            this.RaisePropertyChanged(nameof(Header));
        }

    }
}
