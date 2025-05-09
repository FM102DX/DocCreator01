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
        
        public SettingsViewModel Settings { get; }

        public ProjectSettingsTabViewModel(Settings settings)
        {
            Settings = new SettingsViewModel(settings);
            
            // Subscribe to Settings.IsDirty changes to update our own dirty state
            this.WhenAnyValue(x => x.Settings.IsDirty)
                .Where(isDirty => isDirty)
                .Subscribe(_ => {
                    IsDirty = true;
                    this.RaisePropertyChanged(nameof(Header));
                });
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set => this.RaiseAndSetIfChanged(ref _isDirty, value);
        }

        public string Header => IsDirty ? $"{TabName} *" : TabName;

        public void AcceptChanges()
        {
            Settings.AcceptChanges();
            IsDirty = false;
            this.RaisePropertyChanged(nameof(Header));
        }

        public void MarkAsDirty()
        {
            IsDirty = true;
            this.RaisePropertyChanged(nameof(Header));
        }
    }
}
