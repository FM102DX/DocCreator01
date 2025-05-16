using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DocCreator01.Contracts;
using DocCreator01.Services;

namespace DocCreator01.ViewModels
{
    public sealed class SettingsViewModel : ReactiveObject, IDirtyTrackable
    {
        public Settings Model { get; }
        private readonly IDirtyStateManager _dirtyStateMgr;
        private readonly IProjectHelper _projectHelper;
        private readonly CompositeDisposable _cleanup = new CompositeDisposable();

        [Reactive] public IEnumerable<HtmlGenerationProfile> HtmlGenerationProfiles { get; set; }
        [Reactive] public HtmlGenerationProfile? SelectedHtmlGenerationProfile { get; set; }

        public bool IsDocxSelected
        {
            get => Model.GenDocType == GenerateFileTypeEnum.DOCX;
            set
            {
                if (value && Model.GenDocType != GenerateFileTypeEnum.DOCX)
                {
                    Model.GenDocType = GenerateFileTypeEnum.DOCX;
                    _dirtyStateMgr.MarkAsDirty();
                    this.RaisePropertyChanged(nameof(IsHtmlSelected));
                }
            }
        }

        public bool IsHtmlSelected
        {
            get => Model.GenDocType == GenerateFileTypeEnum.HTML;
            set
            {
                if (value && Model.GenDocType != GenerateFileTypeEnum.HTML)
                {
                    Model.GenDocType = GenerateFileTypeEnum.HTML;
                    _dirtyStateMgr.MarkAsDirty();
                    this.RaisePropertyChanged(nameof(IsDocxSelected));
                }
            }
        }

        public SettingsViewModel(Settings model, IProjectHelper projectHelper, IDirtyStateManager dirtyStateMgr = null)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _projectHelper = projectHelper ?? throw new ArgumentNullException(nameof(projectHelper));
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();

            // Load HTML Generation Profiles using the injected IProjectHelper instance
            HtmlGenerationProfiles = _projectHelper.GetHtmlGenerationProfiles();
            
            // Initialize the selected profile:
            // 1. If CurrentHtmlGenerationProfile object is already set in the model, use it.
            // 2. Else, if CurrentHtmlGenerationProfileId is set, try to find the profile in the loaded list.
            // 3. Else, default to the first profile in the list.
            if (Model.CurrentHtmlGenerationProfile == null && Model.CurrentHtmlGenerationProfileId != 0 && HtmlGenerationProfiles != null)
            {
                Model.CurrentHtmlGenerationProfile = HtmlGenerationProfiles.FirstOrDefault(p => p.Id == Model.CurrentHtmlGenerationProfileId);
            }
            SelectedHtmlGenerationProfile = Model.CurrentHtmlGenerationProfile ?? HtmlGenerationProfiles?.FirstOrDefault();

            // When SelectedHtmlGenerationProfile (UI selection) changes, update the model
            this.WhenAnyValue(x => x.SelectedHtmlGenerationProfile)
                .Skip(1) // Skip initial set, only react to user changes
                .Subscribe(profile =>
                {
                    if (Model.CurrentHtmlGenerationProfile != profile) // Update only if changed
                    {
                        Model.CurrentHtmlGenerationProfile = profile;
                        Model.CurrentHtmlGenerationProfileId = profile?.Id ?? 0;
                        _dirtyStateMgr.MarkAsDirty();
                    }
                })
                .DisposeWith(_cleanup);

            // When Model.GenDocType changes, update radio button states
            this.WhenAnyValue(x => x.Model.GenDocType)
                .ObserveOn(RxApp.MainThreadScheduler) // Ensure UI properties are updated on the UI thread
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(IsDocxSelected));
                    this.RaisePropertyChanged(nameof(IsHtmlSelected));
                })
                .DisposeWith(_cleanup);
        }

        public void Dispose() => _cleanup.Dispose();

        // IDirtyTrackable implementation
        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;
    }
}
