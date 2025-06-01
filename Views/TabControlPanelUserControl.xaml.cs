using DocCreator01.Contracts;
using DocCreator01.ViewModels;
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
            if (sender is TabItem tabItem)
            {
                ShowTabContextMenu(tabItem);
            }
        }

        public void ShowTabContextMenu(TabItem tabItem)
        {
            if (Resources["TabContextMenu"] is ContextMenu contextMenu)
            {
                contextMenu.PlacementTarget = tabItem;
                contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }
        }
    }
}
