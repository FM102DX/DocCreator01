using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DocCreator01.Converters
{
    /// <summary>
    /// Конвертирует byte array в BitmapSource для отображения изображений в WPF
    /// </summary>
    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] imageData && imageData.Length > 0)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(imageData);
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
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}