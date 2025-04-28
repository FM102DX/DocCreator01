using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.ViewModels
{
    public sealed class ProjectViewModel : ReactiveObject
    {
        public Project Model { get; }

        public SettingsViewModel Settings { get; }
        public ProjectDataViewModel ProjectData { get; }

        public ProjectViewModel(Project model)
        {
            Model = model;
            Settings = new SettingsViewModel(Model.Settings);
            ProjectData = new ProjectDataViewModel(Model.ProjectData);
        }

        /* Если понадобятся свойства верхнего уровня: */
        // public string Name
        // {
        //     get => Model.Name;
        //     set => this.RaiseAndSetIfChanged(ref Model.Name, value);
        // }
    }
}
