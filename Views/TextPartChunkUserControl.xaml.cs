using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DocCreator01.Models;
using DocCreator01.ViewModels;

namespace DocCreator01.Views
{
    public partial class TextPartChunkUserControl : UserControl
    {
        public TextPartChunkUserControl()
        {
            InitializeComponent();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is not TextPartChunkViewModel chunkVm) 
                return;
            
            // Найдем TabPageViewModel в родительских элементах
            var tpVm = FindTabPageViewModel();
            
            if (tpVm != null)
            {
                tpVm.EnsureTrailingEmptyChunk(chunkVm);
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 1) Нужна ровно клавиша V
            if (e.Key != Key.V) return;

            // 2) И одновременно зажат Ctrl (без Shift, Alt и пр.)
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;

            // 3) Есть ли что вставлять?
            if (!Clipboard.ContainsImage() && !Clipboard.ContainsFileDropList())
                return;

            HandlePasteImage();   // ваш метод вставки
            e.Handled = true;     // гасим стандартную вставку текста
        }

        private void OnRemoveImageClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is TextPartChunkViewModel chunkVm)
            {
                chunkVm.ClearImage();
            }
        }
        
        private void HandlePasteImage()
        {
            try
            {
                if (!Clipboard.ContainsImage())
                    return;
                    
                var image = Clipboard.GetImage();
                if (image == null)
                    return;
                    
                if (DataContext is not TextPartChunkViewModel chunkVm)
                    return;
                    
                // Конвертируем изображение в byte array
                var originalImageData = ConvertBitmapSourceToByteArray(image);
                
                // Создаем миниатюру
                var thumbnail = CreateThumbnail(image, 200, 150);
                var thumbnailData = ConvertBitmapSourceToByteArray(thumbnail);
                
                // Сохраняем в чанк
                chunkVm.SetImage(originalImageData, thumbnailData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при вставке изображения: {ex.Message}", 
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private byte[] ConvertBitmapSourceToByteArray(BitmapSource bitmapSource)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
        
        private BitmapSource CreateThumbnail(BitmapSource source, int maxWidth, int maxHeight)
        {
            double scaleX = (double)maxWidth / source.PixelWidth;
            double scaleY = (double)maxHeight / source.PixelHeight;
            double scale = Math.Min(scaleX, scaleY);
            
            if (scale >= 1.0)
                return source; // Изображение уже меньше максимального размера
                
            int newWidth = (int)(source.PixelWidth * scale);
            int newHeight = (int)(source.PixelHeight * scale);
            
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scale, scale));
            
            var transformedBitmap = new TransformedBitmap(source, transformGroup);
            
            // Создаем RenderTargetBitmap для финального изображения
            var renderTarget = new RenderTargetBitmap(newWidth, newHeight, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();
            
            using (var context = visual.RenderOpen())
            {
                context.DrawImage(transformedBitmap, new Rect(0, 0, newWidth, newHeight));
            }
            
            renderTarget.Render(visual);
            renderTarget.Freeze();
            
            return renderTarget;
        }
        
        private TabPageViewModel? FindTabPageViewModel()
        {
            // Пробуем найти через логическое дерево
            DependencyObject current = this;
            while (current != null)
            {
                if (current is ListView listView && listView.DataContext is TabPageViewModel tpVm)
                {
                    return tpVm;
                }
                
                if (current is FrameworkElement fe && fe.DataContext is TabPageViewModel viewModel)
                {
                    return viewModel;
                }
                
                current = LogicalTreeHelper.GetParent(current);
            }
            
            // Если не нашли через логическое дерево, пробуем через визуальное
            current = this;
            while (current != null)
            {
                if (current is FrameworkElement visualFe && visualFe.DataContext is TabPageViewModel visualTpVm)
                {
                    return visualTpVm;
                }
                
                current = VisualTreeHelper.GetParent(current);
            }
            
            return null;
        }
    }
}
