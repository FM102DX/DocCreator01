using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using DocCreator01.Data.Enums;
using System.Reactive.Disposables;
using DocCreator01.Contracts;
using DocCreator01.Services;
using ReactiveUI.Fody.Helpers;

namespace DocCreator01.ViewModels
{
    public sealed class ProjectSettingsTabViewModel : ReactiveObject, ITabViewModel, IDisposable, IDirtyTrackable
    {
        private readonly Project _project;
        private readonly CompositeDisposable _cleanup = new();
        private readonly IDirtyStateManager _dirtyStateMgr;
        private const string BaseHeader = "Project settings";

        public ProjectSettingsTabViewModel(Project project, IDirtyStateManager dirtyStateMgr = null, IProjectHelper projectHelper = null)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();
            
            SettingsVm = new SettingsViewModel(
                _project.Settings, 
                projectHelper ?? throw new ArgumentNullException(nameof(projectHelper)), 
                new DirtyStateManager()); // Use a separate dirty manager for SettingsViewModel
            
            _dirtyStateMgr.AddSubscription(SettingsVm);
            
            Name = _project.Name;
            DocTitle = _project.Settings.DocTitle;
            DocDescription = _project.Settings.DocDescription;
            DocCreatedBy = _project.Settings.DocCretaedBy;
            
            this.WhenAnyValue(vm => vm.Name)
                .Skip(1)
                .Subscribe(v => {
                    _project.Name = v;
                    _dirtyStateMgr.MarkAsDirty();
                })
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.DocTitle)
                .Skip(1)
                .Subscribe(v => {
                    _project.Settings.DocTitle = v;
                    _dirtyStateMgr.MarkAsDirty();
                })
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.DocDescription)
                .Skip(1)
                .Subscribe(v => {
                    _project.Settings.DocDescription = v;
                    _dirtyStateMgr.MarkAsDirty();
                })
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.DocCreatedBy)
                .Skip(1)
                .Subscribe(v => {
                    _project.Settings.DocCretaedBy = v;
                    _dirtyStateMgr.MarkAsDirty();
                })
                .DisposeWith(_cleanup);

            _dirtyStateMgr.IBecameDirty += () =>
                this.RaisePropertyChanged(nameof(TabHeader));
            _dirtyStateMgr.DirtryStateWasReset += () =>
                this.RaisePropertyChanged(nameof(TabHeader));
        }

        // SettingsViewModel to handle GenDocType and HTML profiles
        [Reactive] public SettingsViewModel SettingsVm { get; set; }

        // Only keep project-specific properties here, HTML generation settings are handled by SettingsVm
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string DocTitle { get; set; } = string.Empty;
        [Reactive] public string DocDescription { get; set; } = string.Empty;
        [Reactive] public string DocCreatedBy { get; set; } = string.Empty;

        public string TabHeader => DirtyStateMgr.IsDirty ? $"{BaseHeader}*" : BaseHeader;

        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;

        public void Dispose() 
        {
            SettingsVm.Dispose();
            _cleanup.Dispose();
        }
    }
}
