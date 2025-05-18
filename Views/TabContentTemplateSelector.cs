using System.Windows;
using System.Windows.Controls;
using DocCreator01.ViewModels;

namespace DocCreator01.Views
{
    public class TabContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextPartTemplate { get; set; }
        public DataTemplate SettingsTemplate { get; set; }
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TabPageViewModel)
                return TextPartTemplate;
                
            if (item is ProjectSettingsTabViewModel)
                return SettingsTemplate;
                
            return base.SelectTemplate(item, container);
        }
    }
}
