using DocCreator01.ViewModel;
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

namespace DocCreator01.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        void TabItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TabItem tab || DataContext is not MainWindowViewModel vm) return;

            var tabVm = tab.DataContext as TabPageViewModel;
            if (tabVm == null) return;

            var menu = new ContextMenu();
            var closeAll = new MenuItem { Header = "Закрыть все" };
            var close = new MenuItem { Header = "Закрыть" };
            var del = new MenuItem { Header = "Удалить" };

            close.Click += (_, _) => vm.CloseTabCommand.Execute(tabVm).Subscribe();
            del.Click += (_, _) => vm.DeleteTabCommand.Execute(tabVm).Subscribe();
            closeAll.Click += (_, _) => vm.CloseAllTabsCommand.Execute().Subscribe();
            menu.Items.Add(close);
            menu.Items.Add(closeAll);
            menu.Items.Add(del);
            menu.IsOpen = true;
        }
    }
}
