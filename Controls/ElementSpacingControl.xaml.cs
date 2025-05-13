using DocCreator01.Models;
using DocCreator01.ViewModels;
using System.Windows.Controls;

namespace DocCreator01.Controls
{
    public partial class ElementSpacingControl : UserControl
    {
        public ElementSpacingViewModel ViewModel { get; }

        public ElementSpacingControl()
        {
            InitializeComponent();
            ViewModel = new ElementSpacingViewModel();
            DataContext = ViewModel;
        }

        public ElementSpacingInfo Spacing
        {
            get => ViewModel.ToDto();
            set => ViewModel.LoadFromDto(value);
        }
    }
}
