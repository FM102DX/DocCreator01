using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DocCreator01.ViewModels;

namespace DocCreator01.Views
{
        public partial class TextPartsPanelUserControl : UserControl
        {
            public TextPartsPanelUserControl() 
            {
                InitializeComponent();
                this.DataContextChanged += TextPartsPanelUserControl_DataContextChanged;
            }

            private void TextPartsPanelUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (e.OldValue is MainWindowViewModel oldViewModel)
                {
                    oldViewModel.MainGridLines.CollectionChanged -= MainGridLines_CollectionChanged;
                }

                if (e.NewValue is MainWindowViewModel newViewModel)
                {
                    newViewModel.MainGridLines.CollectionChanged += MainGridLines_CollectionChanged;
                    UpdateParagraphNumberColumnWidth(newViewModel.MainGridLines);
                }
            }

            private void MainGridLines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    UpdateParagraphNumberColumnWidth(viewModel.MainGridLines);
                }
            }

            private void UpdateParagraphNumberColumnWidth(System.Collections.ObjectModel.ObservableCollection<MainGridItemViewModel> items)
            {
                if (items == null || items.Count == 0)
                    return;

                string longestNumber = items.Select(i => i.ParagraphNo ?? string.Empty).OrderByDescending(s => s.Length).First();
                double width = CalculateTextWidth(
                    longestNumber, 
                    "Segoe UI", 
                    12, 
                    FontWeights.SemiBold);

                // Add some padding for the margin
                width += 20; // Accounting for left margin (4px) and right margin (8px) plus additional buffer

                if (PartsGrid.Columns.Count > 0)
                {
                    PartsGrid.Columns[0].Width = new DataGridLength(width);
                }
            }
            
            private double CalculateTextWidth(string text, string fontFamily, double fontSize, FontWeight fontWeight)
            {
                // Упрощенный расчет: используем среднюю ширину символа для 14 шрифта
                // Для шрифта Segoe UI размером 12-14 пикселей, средняя ширина символа составляет примерно 7-8 пикселей
                const double averageCharWidth = 8.0;
                return text.Length * averageCharWidth;
            }
        }
}
