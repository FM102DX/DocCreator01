using DocCreator01.Contracts;
using DocCreator01.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

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

            Text = model.Text ?? string.Empty;

            this.WhenAnyValue(vm => vm.Text)
                .DistinctUntilChanged()
                .Subscribe(v =>
                {
                    Model.Text = v;
                    _dirtyStateMgr?.MarkAsDirty();
                });
        }

        [Reactive] public string Text { get; set; } = string.Empty;

        // expose Id if needed elsewhere
        public Guid Id => Model.Id;
        
        /// <summary>
        /// Проверяет, содержит ли чанк изображение
        /// </summary>
        public bool HasImage => Model.HasImage;
        
        /// <summary>
        /// Миниатюра изображения для отображения в UI
        /// </summary>
        public BitmapSource? ThumbnailImage
        {
            get
            {
                if (Model.ThumbnailData == null || Model.ThumbnailData.Length == 0)
                    return null;
                    
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new System.IO.MemoryStream(Model.ThumbnailData);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        /// <summary>
        /// Устанавливает изображение в чанк
        /// </summary>
        public void SetImage(byte[] imageData, byte[] thumbnailData)
        {
            Model.ImageData = imageData;
            Model.ThumbnailData = thumbnailData;
            
            this.RaisePropertyChanged(nameof(HasImage));
            this.RaisePropertyChanged(nameof(ThumbnailImage));
            _dirtyStateMgr?.MarkAsDirty();
        }
        
        /// <summary>
        /// Удаляет изображение из чанка
        /// </summary>
        public void ClearImage()
        {
            Model.ImageData = null;
            Model.ThumbnailData = null;
            
            this.RaisePropertyChanged(nameof(HasImage));
            this.RaisePropertyChanged(nameof(ThumbnailImage));
            _dirtyStateMgr?.MarkAsDirty();
        }
    }
}
