using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using DocCreator01.Data.Enums;
using System.Reactive.Disposables;
using System.Reactive;
using DocCreator01.Contracts;
using ReactiveUI.Fody.Helpers;

namespace DocCreator01.ViewModels
{
    public sealed class ProjectSettingsTabViewModel : ReactiveObject, ITabViewModel, IDisposable
    {
        private readonly Project _project;
        private readonly CompositeDisposable _cleanup = new();

        private const string BaseHeader = "Project settings";

        // ──────────────── ctor ────────────────────────────────────────────────
        public ProjectSettingsTabViewModel(Project project)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));

            // начальная загрузка VM из модели
            Name = _project.Name;
            GenDocType = _project.Settings.GenDocType;
            DocTitle = _project.Settings.DocTitle;
            DocDescription = _project.Settings.DocDescription;
            DocCreatedBy = _project.Settings.DocCretaedBy;
             // синхронизация VM → Model
            this.WhenAnyValue(vm => vm.Name)
                .Skip(1)
                .Subscribe(v => _project.Name = v)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.GenDocType)
                .Skip(1)
                .Subscribe(v => _project.Settings.GenDocType = v)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.DocTitle)
                .Skip(1)
                .Subscribe(v => _project.Settings.DocTitle = v)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.DocDescription)
                .Skip(1)
                .Subscribe(v => _project.Settings.DocDescription = v)
                .DisposeWith(_cleanup);

            this.WhenAnyValue(vm => vm.DocCreatedBy)
                .Skip(1)
                .Subscribe(v => _project.Settings.DocCretaedBy = v)
                .DisposeWith(_cleanup);

            // любые изменения (кроме IsDirty) → IsDirty = true
            this.Changed
                .Where(e => e.PropertyName != nameof(IsDirty))
                .Subscribe(_ => IsDirty = true)
                .DisposeWith(_cleanup);

            // TabHeader = BaseHeader или BaseHeader*
            this.WhenAnyValue(vm => vm.IsDirty)
                .Select(d => d ? $"{BaseHeader}*" : BaseHeader)
                .ToProperty(this, vm => vm.TabHeader, out _tabHeader)
                .DisposeWith(_cleanup);
        }

        // ─────────── редактируемые reactive-поля ──────────────────────────────
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public GenerateFileTypeEnum GenDocType { get; set; }
        [Reactive] public string DocTitle { get; set; } = string.Empty;
        [Reactive] public string DocDescription { get; set; } = string.Empty;
        [Reactive] public string DocCreatedBy { get; set; } = string.Empty;

        // ──────────── ITabViewModel implementation ────────────────────────────
        private readonly ObservableAsPropertyHelper<string> _tabHeader;
        public string TabHeader => _tabHeader.Value;

        [Reactive] public bool IsDirty { get; private set; }

        public void AcceptChanges() => IsDirty = false;

        public void MarkAsDirty() => IsDirty = true;
        // ───────────────────────────────────────────────────────────────────────

        public Project Model => _project;

        public void Dispose() => _cleanup.Dispose();
    }
}
