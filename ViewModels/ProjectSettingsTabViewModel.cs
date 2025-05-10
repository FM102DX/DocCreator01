using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using DocCreator01.Data.Enums;
using System.Reactive.Disposables;
using System.Reactive;
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

        public ProjectSettingsTabViewModel(Project project, IDirtyStateManager dirtyStateMgr = null)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();

            // начальная загрузка VM из модели
            Name = _project.Name;
            GenDocType = _project.Settings.GenDocType;
            DocTitle = _project.Settings.DocTitle;
            DocDescription = _project.Settings.DocDescription;
            DocCreatedBy = _project.Settings.DocCretaedBy;
            
            // синхронизация VM → Model
            this.WhenAnyValue(vm => vm.Name)
                .Skip(1)
                .Subscribe(v => {
                    _project.Name = v;
                    _dirtyStateMgr.MarkAsDirty();
                })
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.GenDocType)
                .Skip(1)
                .Subscribe(v => {
                    _project.Settings.GenDocType = v;
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

            this.WhenAnyValue(x => x.DirtyStateMgr.IsDirty)
                .Select(d => d ? $"{BaseHeader}*" : BaseHeader)
                .ToProperty(this, vm => vm.TabHeader, out _tabHeader)
                .DisposeWith(_cleanup);

            _dirtyStateMgr.IBecameDirty += () =>
                this.RaisePropertyChanged(nameof(TabHeader));
            _dirtyStateMgr.DirtryStateWasReset += () =>
                this.RaisePropertyChanged(nameof(TabHeader));
        }

        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public GenerateFileTypeEnum GenDocType { get; set; }
        [Reactive] public string DocTitle { get; set; } = string.Empty;
        [Reactive] public string DocDescription { get; set; } = string.Empty;
        [Reactive] public string DocCreatedBy { get; set; } = string.Empty;

        private readonly ObservableAsPropertyHelper<string> _tabHeader;
        public string TabHeader => _tabHeader.Value;

        public bool IsDirty => DirtyStateMgr.IsDirty;

        public void AcceptChanges() => DirtyStateMgr.ResetDirtyState();

        public void MarkAsDirty() => DirtyStateMgr.MarkAsDirty();

        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;

        public Project Model => _project;

        public void Dispose() => _cleanup.Dispose();
    }
}
