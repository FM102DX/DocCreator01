using DocCreator01.Contracts;
using DocCreator01.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocCreator01.Views
{
    /// <summary>
    /// Interaction logic for TabControlPanelUserControl.xaml
    /// </summary>
    public partial class TabControlPanelUserControl : UserControl
    {
        public TabControlPanelUserControl()
        {
            InitializeComponent();
        }

        private void TabItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TabItem;
            if (item != null && item.DataContext is ITabViewModel)
            {
                var cm = Resources["TabContextMenu"] as ContextMenu;
                if (cm != null)
                {
                    cm.DataContext = item.DataContext;
                    cm.IsOpen = true;
                    e.Handled = true;
                }
            }
        }
    }
}
