using DocCreator01.ViewModel;
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

namespace DocCreator01
{

    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        void TabItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TabItem tab || DataContext is not MainWindowViewModel vm) return;

            var tabVm = tab.DataContext as TabPageViewModel;
            if (tabVm == null) return;

            var menu = new ContextMenu();
            var close = new MenuItem { Header = "Закрыть" };
            var del = new MenuItem { Header = "Удалить" };

            close.Click += (_, _) => vm.CloseTabCommand.Execute(tabVm).Subscribe();
            del.Click += (_, _) => vm.DeleteTabCommand.Execute(tabVm).Subscribe();

            menu.Items.Add(close);
            menu.Items.Add(del);
            menu.IsOpen = true;
        }
    }

}
