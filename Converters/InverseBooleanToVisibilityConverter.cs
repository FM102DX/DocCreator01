using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DocCreator01.Converters
{
    /// <summary>
    /// Returns Visible when the bound boolean is false, otherwise Collapsed.
    /// </summary>
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = value as bool? ?? false;
            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v != Visibility.Visible;
            return Binding.DoNothing;
        }
    }
}
