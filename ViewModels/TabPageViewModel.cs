using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using DocCreator01.Contracts;
using DocCreator01.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DocCreator01.ViewModels
{
    public sealed class TabPageViewModel : ReactiveObject, ITabViewModel, IDirtyTrackable
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
                model.TextPartChunks.Select(c => new TextPartChunkViewModel(c, _dirtyStateMgr)));

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

        /// <summary>
        /// Проверяет редактируемый последний элемент и добавляет новый пустой,
        /// если необходимо.
        /// </summary>
        public void EnsureTrailingEmptyChunk(TextPartChunkViewModel editedChunk)
        {
            if (editedChunk == null) return;
            
            // Получаем последний элемент
            var lastChunk = TextPartChunks.Count > 0 ? TextPartChunks[TextPartChunks.Count - 1] : null;
            
            // Проверяем совпадение по ID вместо ссылки
            bool isLastChunk = lastChunk != null && lastChunk.Id == editedChunk.Id;
            
            if (!isLastChunk) return;
            
            // Если редактируемый последний чанк пуст, ничего не делаем
            if (string.IsNullOrWhiteSpace(editedChunk.Text)) return;
            
            // Принудительно добавляем новый чанк через ProjectHelper
            var newChunkModel = _projectHelper.AddEmptyChunk(Model);
            
            if (newChunkModel != null)
            {
                // Добавляем соответствующий ViewModel для нового чанка
                TextPartChunks.Add(new TextPartChunkViewModel(newChunkModel, _dirtyStateMgr));
                
                // Отмечаем документ как измененный
                _dirtyStateMgr?.MarkAsDirty();
            }
        }

        public void RemoveChunk(TextPartChunkViewModel chunk)
        {
            if (chunk == null) return;
            if (!TextPartChunks.Contains(chunk)) return;

            // Ищем соответствующий элемент модели только по ID
            var modelChunk = Model.TextPartChunks.FirstOrDefault(c => c.Id == chunk.Id);
            
            // Если нашли модельный чанк - удаляем его
            if (modelChunk != null)
            {
                Model.TextPartChunks.Remove(modelChunk);
            }
            
            // В любом случае удаляем из ViewModel
            TextPartChunks.Remove(chunk);
            
            // Маркируем как измененный
            _dirtyStateMgr?.MarkAsDirty();
            
            // Используем ProjectHelper для добавления пустого чанка если нужно
            var newChunkModel = _projectHelper.AddEmptyChunkIfNeeded(Model);
            
            if (newChunkModel != null)
            {
                TextPartChunks.Add(new TextPartChunkViewModel(newChunkModel, _dirtyStateMgr));
            }
        }
    }
}