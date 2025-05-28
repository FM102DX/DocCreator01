using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DocCreator01.Helpers
{
    public static class TextWidthCalculator
    {
        public static double CalculateTextWidth(string text, string fontFamily, double fontSize, FontWeight fontWeight)
        {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, fontWeight, FontStretches.Normal),
                fontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

            return formattedText.Width;
        }
    }
}