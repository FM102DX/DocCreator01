using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using DocCreator01.Contracts;
using DocCreator01.Models;
using DocCreator01.Services;      // new
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DocCreator01.ViewModels
{
    public sealed partial class TabPageViewModel : ReactiveObject, ITabViewModel, IDirtyTrackable
    {
        private readonly IDirtyStateManager _dirtyStateMgr;
        private readonly IProjectHelper _projectHelper;

        public TabPageViewModel(TextPart model, IDirtyStateManager dirtyStateMgr, IProjectHelper projectHelper)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _dirtyStateMgr = dirtyStateMgr ?? throw new ArgumentNullException(nameof(dirtyStateMgr));
            _projectHelper = projectHelper ?? throw new ArgumentNullException(nameof(projectHelper));

            Name = model.Name;
            Text = model.Text;
            IncludeInDocument = model.IncludeInDocument;

            // Гарантируем, что коллекция TextPartChunks существует в модели
            if (model.TextPartChunks == null)
                model.TextPartChunks = new List<TextPartChunk>();

            // Преобразуем коллекцию модели в коллекцию ViewModel
            TextPartChunks = new ObservableCollection<TextPartChunkViewModel>(
                model.TextPartChunks.Select(c => CreateChunkVm(c)));

            // Гарантируем наличие пустого хвостового элемента
            if (TextPartChunks.Count == 0 || !string.IsNullOrWhiteSpace(TextPartChunks.Last().Text))
            {
                var emptyModel = new TextPartChunk { Id = Guid.NewGuid(), Text = string.Empty };
                model.TextPartChunks.Add(emptyModel);
                TextPartChunks.Add(new TextPartChunkViewModel(emptyModel, _dirtyStateMgr));
            }

            TextPartChunks.CollectionChanged += (_, __) => _dirtyStateMgr.MarkAsDirty();
            /* ----------------------------------------------------------- */

            _dirtyStateMgr.IBecameDirty += () => this.RaisePropertyChanged(nameof(TabHeader));
            _dirtyStateMgr.DirtryStateWasReset += () => this.RaisePropertyChanged(nameof(TabHeader));

            DeleteChunkCommand = ReactiveCommand.Create<TextPartChunkViewModel>(RemoveChunk);
        }

        public string TabHeader => _dirtyStateMgr.IsDirty ? $"{Name ?? "Untitled"} *" : Name ?? "Untitled";

        public TextPart Model { get; }

        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;

        [Reactive] public string Name { get; set; }
        [Reactive] public string Text { get; set; }
        [Reactive] public bool IncludeInDocument { get; set; }

        // NEW: collection for ListView / ListBox in TextPartUserControl
        public ObservableCollection<TextPartChunkViewModel> TextPartChunks { get; }

        public ICommand DeleteChunkCommand { get; }

        private TextPartChunkViewModel CreateChunkVm(TextPartChunk chunkModel)
        {
            var vm = new TextPartChunkViewModel(chunkModel, _dirtyStateMgr);

            // подписываемся на любые изменения изображения
            vm.ImageCleared  += OnChunkImageChanged;   // legacy
            vm.ImageChanged  += OnChunkImageChanged;   // new
            return vm;
        }

        private void OnChunkImageChanged(TextPartChunkViewModel chunk)
            => EnsureTrailingEmptyChunk(chunk);

        /* ---- always keep an empty trailing chunk ---- */
        public void EnsureTrailingEmptyChunk(TextPartChunkViewModel? /*editedChunk not needed*/ _)
        {
            if (TextPartChunks.Count == 0)
            {
                var m = TextPartHelper.AddEmptyChunk(Model);
                if (m != null) TextPartChunks.Add(CreateChunkVm(m));
                return;
            }

            var lastVm = TextPartChunks[^1];
            if (!TextPartHelper.IsChunkEmpty(lastVm.Model))
            {
                var m = TextPartHelper.AddEmptyChunk(Model);
                if (m != null) TextPartChunks.Add(CreateChunkVm(m));
                _dirtyStateMgr?.MarkAsDirty();
            }
        }

        private void OnChunkImageCleared(TextPartChunkViewModel vm)
            => EnsureTrailingEmptyChunk(vm);

 
        public void RemoveChunk(TextPartChunkViewModel chunk)
        {
            if (chunk == null) return;
            
            // Новый блок: подтверждение удаления непустого чанка
            if (!string.IsNullOrWhiteSpace(chunk.Text))
            {
                var result = MessageBox.Show(
                    "Удалить выбранный блок текста?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;     // отмена удаления
            }

            if (!TextPartChunks.Contains(chunk)) return;
            var modelChunk = Model.TextPartChunks.FirstOrDefault(c => c.Id == chunk.Id);
            if (modelChunk != null)
            {
                Model.TextPartChunks.Remove(modelChunk);
            }
            TextPartChunks.Remove(chunk);
            _dirtyStateMgr?.MarkAsDirty();
            var newChunk = TextPartHelper.AddEmptyChunkIfNeeded(Model);
            if (newChunk != null)
                TextPartChunks.Add(CreateChunkVm(newChunk));
        }
    }
}