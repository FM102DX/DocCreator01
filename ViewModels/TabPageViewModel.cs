using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DocCreator01.Contracts;
using DocCreator01.Models;
using DocCreator01.Services;
using ReactiveUI;

namespace DocCreator01.ViewModels
{
    public sealed class TabPageViewModel : ReactiveObject, ITabViewModel, IDirtyTrackable
    {
        private TextPart? _textPart;
        private readonly IDirtyStateManager _dirtyStateMgr;

        public string? Name => TextPart?.Name;

       public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;
 
        public string TabHeader => DirtyStateMgr.IsDirty ? $"{TextPart?.Name ?? "Untitled"} *" : TextPart?.Name ?? "Untitled";

        public TabPageViewModel(TextPart? textPart, IDirtyStateManager dirtyStateMgr = null)
        {
            this._textPart = textPart;
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();

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
                        _dirtyStateMgr.MarkAsDirty();
                        this.RaisePropertyChanged(nameof(TabHeader));
                    },
                    // Add error handler to prevent unhandled exceptions
                    ex =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in TabPageViewModel subscription: {ex.Message}");
                    });
            }
            _dirtyStateMgr.IBecameDirty += () =>
                this.RaisePropertyChanged(nameof(TabHeader));
            _dirtyStateMgr.DirtryStateWasReset += () =>
                this.RaisePropertyChanged(nameof(TabHeader));

        }

        public TextPart? TextPart
        {
            get => _textPart;
            set
            {
                this.RaiseAndSetIfChanged(ref _textPart, value);

                if (value != null)
                {
                    this.WhenAnyValue(_ => _.TextPart.Name,
                            _ => _.TextPart.Text,
                            _ => _.TextPart.Level,
                            _ => _.TextPart.IncludeInDocument)
                        .Skip(1)
                        .Subscribe(_ =>
                        {
                            _dirtyStateMgr.MarkAsDirty();
                            this.RaisePropertyChanged(nameof(TabHeader));
                        },
                        ex =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in TextPart subscription: {ex.Message}");
                        });
                }
            }
        }
    }
}
