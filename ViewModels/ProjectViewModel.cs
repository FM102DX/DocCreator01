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
        public Project Project { get; }

        public SettingsViewModel Settings { get; }
        public ProjectDataViewModel ProjectData { get; }

        public ProjectViewModel(Project project)
        {
            Project = project;
            Settings = new SettingsViewModel(Project.Settings);
            ProjectData = new ProjectDataViewModel(Project.ProjectData);
        }
    }
}
