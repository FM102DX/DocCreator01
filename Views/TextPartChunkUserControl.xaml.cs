using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            TabPageViewModel tpVm = FindTabPageViewModel();
            
            if (tpVm != null)
            {
                tpVm.EnsureTrailingEmptyChunk(chunkVm);
            }
        }
        
        private TabPageViewModel FindTabPageViewModel()
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
