using DocCreator01.ViewModels;
using DocCreator01.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using System.Reactive;

namespace DocCreator01.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked item
            var listBox = (ListBox)sender;
            var clickedItem = listBox.SelectedItem as GeneratedFileViewModel;

            if (clickedItem == null)
                return;

            // Open file based on its type
            if (clickedItem.IsHtmlFile || clickedItem.IsNotepadCompatibleFile)
            {
                ((ICommand)clickedItem.OpenInNotepadPlusPlusCommand).Execute(null);
            }
            else if (clickedItem.IsDocxFile)
            {
                ((ICommand)clickedItem.OpenInWordCommand).Execute(null);
            }
        }
    }
}
