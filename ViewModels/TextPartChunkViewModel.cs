using DocCreator01.Contracts;
using DocCreator01.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;

namespace DocCreator01.ViewModels
{
    public sealed class TextPartChunkViewModel : ReactiveObject
    {
        private readonly IDirtyStateManager _dirtyStateMgr;
        public TextPartChunk Model { get; }

        public TextPartChunkViewModel(TextPartChunk model, IDirtyStateManager dirtyStateMgr)
        {
            Model          = model ?? throw new ArgumentNullException(nameof(model));
            _dirtyStateMgr = dirtyStateMgr;

            Text = model.Text;

            this.WhenAnyValue(vm => vm.Text)
                .DistinctUntilChanged()
                .Subscribe(v =>
                {
                    Model.Text = v;
                    _dirtyStateMgr?.MarkAsDirty();
                });
        }

        [Reactive] public string Text { get; set; }

        // expose Id if needed elsewhere
        public Guid Id => Model.Id;
    }
}
