using System;
using System.Reactive.Linq;
using DocCreator01.Contracts;
using DocCreator01.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DocCreator01.ViewModels
{
    public sealed class TabPageViewModel : ReactiveObject, ITabViewModel, IDirtyTrackable
    {
        private readonly IDirtyStateManager _dirtyStateMgr;

        public TabPageViewModel(TextPart model, IDirtyStateManager dirtyStateMgr)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _dirtyStateMgr = dirtyStateMgr ?? throw new ArgumentNullException(nameof(dirtyStateMgr));

            Name = model.Name;
            Text = model.Text;
            IncludeInDocument = model.IncludeInDocument;

            this.WhenAnyValue(vm => vm.Name)
                .DistinctUntilChanged()
                .Subscribe(val =>
                {
                    Model.Name = val;
                    _dirtyStateMgr.MarkAsDirty();
                });

            this.WhenAnyValue(vm => vm.Text)
                .DistinctUntilChanged()
                .Subscribe(val =>
                {
                    Model.Text = val;
                    _dirtyStateMgr.MarkAsDirty();
                });
            this.WhenAnyValue(vm => vm.IncludeInDocument)
                .DistinctUntilChanged()
                .Subscribe(val =>
                {
                    Model.IncludeInDocument = val;
                    _dirtyStateMgr.MarkAsDirty();
                });

            _dirtyStateMgr.IBecameDirty += () => this.RaisePropertyChanged(nameof(TabHeader));
            _dirtyStateMgr.DirtryStateWasReset += () => this.RaisePropertyChanged(nameof(TabHeader));
        }

        public string TabHeader => _dirtyStateMgr.IsDirty ? $"{Name ?? "Untitled"} *" : Name ?? "Untitled";

        public TextPart Model { get; }

        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;

        [Reactive] public string Name { get; set; }
        [Reactive] public string Text { get; set; }
        [Reactive] public bool IncludeInDocument { get; set; }
        
    }
}