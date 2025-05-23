using System;
using System.Collections.Generic;
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
using DocCreator01.Models;

namespace DocCreator01.Views
{
    public partial class TextPartUserControl : UserControl
    {
        public TextPartUserControl() => InitializeComponent();

        private void TextPartChunkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            if (tb.DataContext is not TextPartChunk chunk) return;
            if (DataContext is not TabPageViewModel vm) return;

            vm.EnsureTrailingEmptyChunk(chunk);
        }
    }
}
