using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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


            // convert model chunks → VM collection
            TextPartChunks = new ObservableCollection<TextPartChunkViewModel>(
                (model.TextPartChunks ?? new List<TextPartChunk>())
                    .Select(c => new TextPartChunkViewModel(c, _dirtyStateMgr)));

            // гарантируем наличие пустого хвостового элемента
            if (TextPartChunks.Count == 0 || !string.IsNullOrWhiteSpace(TextPartChunks.Last().Text))
            {
                var emptyModel = new TextPartChunk();
                model.TextPartChunks.Add(emptyModel);
                TextPartChunks.Add(new TextPartChunkViewModel(emptyModel, _dirtyStateMgr));
            }

            TextPartChunks.CollectionChanged += (_, __) => _dirtyStateMgr.MarkAsDirty();
            /* ----------------------------------------------------------- */

            _dirtyStateMgr.IBecameDirty += () => this.RaisePropertyChanged(nameof(TabHeader));
            _dirtyStateMgr.DirtryStateWasReset += () => this.RaisePropertyChanged(nameof(TabHeader));
        }

        public string TabHeader => _dirtyStateMgr.IsDirty ? $"{Name ?? "Untitled"} *" : Name ?? "Untitled";

        public TextPart Model { get; }

        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;

        [Reactive] public string Name { get; set; }
        [Reactive] public string Text { get; set; }
        [Reactive] public bool IncludeInDocument { get; set; }

        // NEW: collection for ListView / ListBox in TextPartUserControl
        public ObservableCollection<TextPartChunkViewModel> TextPartChunks { get; }

        /// <summary>
        /// Проверяет редактируемый последний элемент и добавляет новый пустой,
        /// если необходимо.
        /// </summary>
        public void EnsureTrailingEmptyChunk(TextPartChunkViewModel editedChunk)
        {
            if (editedChunk == null) return;
            if (!ReferenceEquals(TextPartChunks.LastOrDefault(), editedChunk)) return;
            if (string.IsNullOrWhiteSpace(editedChunk.Text)) return;

            var newChunkModel = new TextPartChunk();   // пустой
            Model.TextPartChunks.Add(newChunkModel);
            TextPartChunks.Add(new TextPartChunkViewModel(newChunkModel, _dirtyStateMgr));
        }
    }
}