using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DocCreator01.ViewModels
{
    public class ProjectSettingsTabViewModel : ReactiveObject, ITabViewModel
    {
        private bool _isDirty;
        private const string TabName = "Project Settings";

        public Project Project { get; private set; }
        public SettingsViewModel SettingsVm { get; private set; }

        public ProjectSettingsTabViewModel(Project project)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            SettingsVm = new SettingsViewModel(project.Settings);

            this.WhenAnyValue(vm => vm.SettingsVm.IsDirty)
                .Where(isDirty => isDirty)
                .Subscribe(_ => MarkAsDirty());

            this.WhenAnyValue(vm => vm.Project.Name)
                .Skip(1)
                .DistinctUntilChanged()
                .Subscribe(name =>
                {
                    MarkAsDirty();
                });

            Project.WhenAnyValue(p => p.Name)
                    .Skip(1)
                    .DistinctUntilChanged()
                    .Subscribe(name =>
                    {
                        MarkAsDirty();
                    });
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set => this.RaiseAndSetIfChanged(ref _isDirty, value);
        }

        public string TabHeader => IsDirty ? $"{TabName} *" : TabName;

        public void AcceptChanges()
        {
            SettingsVm.AcceptChanges();
            IsDirty = false;
            this.RaisePropertyChanged(nameof(TabHeader));
        }

        public void MarkAsDirty()
        {
            IsDirty = true;
            this.RaisePropertyChanged(nameof(TabHeader));
        }
    }
}
