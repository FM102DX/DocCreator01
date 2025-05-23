using System.Windows;
using System.Windows.Controls;
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
            if (DataContext is not TextPartChunk chunk) return;

            // walk up the logical tree until we find a DataContext of TabPageViewModel
            DependencyObject current = this;
            while (current != null)
            {
                if (current is FrameworkElement fe && fe.DataContext is TabPageViewModel tpVm)
                {
                    tpVm.EnsureTrailingEmptyChunk(chunk);
                    break;
                }
                current = LogicalTreeHelper.GetParent(current);
            }
        }
    }
}
