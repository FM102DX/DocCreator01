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
    public sealed class TabPageViewModel : ReactiveObject, ITabViewModel
    {
        bool _isDirty;
        private TextPart? _textPart;
        public string? Name => TextPart?.Name;
        public bool IsDirty
        {
            get => _isDirty;
            private set => this.RaiseAndSetIfChanged(ref _isDirty, value);
        }

        // заголовок для вкладки (с * если IsDirty)
        public string Header => IsDirty ? $"{TextPart?.Name ?? "Untitled"} *" : TextPart?.Name ?? "Untitled";

        public TabPageViewModel(TextPart? textPart)
        {
            this._textPart = textPart;

            // Only subscribe if TextPart is not null
            if (textPart != null)
            {
                this.WhenAnyValue(_ => _.TextPart.Name,
                        _ => _.TextPart.Text,
                        _ => _.TextPart.Level,
                        _ => _.TextPart.IncludeInDocument)
                    .Skip(1)
                    .Subscribe(_ =>
                    {
                        IsDirty = true;
                        this.RaisePropertyChanged(nameof(Header));
                    },
                    // Add error handler to prevent unhandled exceptions
                    ex =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in TabPageViewModel subscription: {ex.Message}");
                        // Optionally handle the error or log it
                    });
            }
        }

        public TextPart? TextPart
        {
            get => _textPart;
            set
            {
                // Unsubscribe from old TextPart if needed (could implement IDisposable pattern)
                this.RaiseAndSetIfChanged(ref _textPart, value);

                // If TextPart changed to a non-null value, we might want to re-subscribe
                if (value != null)
                {
                    this.WhenAnyValue(_ => _.TextPart.Name,
                            _ => _.TextPart.Text,
                            _ => _.TextPart.Level,
                            _ => _.TextPart.IncludeInDocument)
                        .Skip(1)
                        .Subscribe(_ =>
                        {
                            IsDirty = true;
                            this.RaisePropertyChanged(nameof(Header));
                        },
                        // Add error handler
                        ex =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in TextPart subscription: {ex.Message}");
                        });
                }
            }
        }

        /// <summary>Вызывается после успешного сохранения проекта.</summary>
        public void AcceptChanges()
        {
            IsDirty = false;
            this.RaisePropertyChanged(nameof(Header));
        }

        /// <summary>Manually marks the tab as dirty</summary>
        public void MarkAsDirty()
        {
            IsDirty = true;
            this.RaisePropertyChanged(nameof(Header));
        }
    }
}
