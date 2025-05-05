using DocCreator01.ViewModel;
using DocCreator01.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocCreator01.Views
{
    public partial class TabControlPanelUserControl : UserControl
    {
        public TabControlPanelUserControl()
        {
            InitializeComponent();
        }

        void TabItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TabItem tab || DataContext is not MainWindowViewModel vm) return;

            var tabVm = tab.DataContext as TabPageViewModel;
            if (tabVm == null) return;

            var menu = new ContextMenu();
            var closeAll = new MenuItem { Header = "������� ���" };
            var close = new MenuItem { Header = "�������" };
            var del = new MenuItem { Header = "�������" };

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
