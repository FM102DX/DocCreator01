using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DocCreator01.ViewModels
{
    public class ProjectSettingsTabViewModel : ReactiveObject, ITabViewModel
    {
        private bool _isDirty;
        private string _name;
        private const string TabName = "Project Settings";

        private readonly Project _project;
        public SettingsViewModel Settings { get; }

        public ProjectSettingsTabViewModel(Project project)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _name = project.Name;
            Settings = new SettingsViewModel(project.Settings);

            /* ── dirty-flag propagation ──────────────────────────────── */
            this.WhenAnyValue(vm => vm.Settings.IsDirty)
                .Where(isDirty => isDirty)
                .Subscribe(_ => MarkAsDirty());

            /* ── VM-to-model (Name changed in UI) ───────────────────── */
            this.WhenAnyValue(vm => vm.Name)
                .Skip(1)
                .DistinctUntilChanged()
                .Subscribe(name =>
                {
                    if (_project.Name != name)
                        _project.Name = name;

                    MarkAsDirty();
                });

            /* ── model-to-VM (Name changed elsewhere) ───────────────── */
            _project.WhenAnyValue(p => p.Name)
                    .Skip(1)
                    .DistinctUntilChanged()
                    .Subscribe(name =>
                    {
                        _name = name;
                        this.RaisePropertyChanged(nameof(Name));
                        MarkAsDirty();
                    });
        }

        /// <summary>
        /// Two–way reactive wrapper around <see cref="_project.Name"/>.
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set => this.RaiseAndSetIfChanged(ref _isDirty, value);
        }

        public string TabHeader => IsDirty ? $"{TabName} *" : TabName;

        public void AcceptChanges()
        {
            Settings.AcceptChanges();
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
