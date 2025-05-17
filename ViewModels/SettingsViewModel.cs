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
        [Reactive] public GenerateFileTypeEnum GenDocType { get; set; }

        public SettingsViewModel(Settings model, IProjectHelper projectHelper, IDirtyStateManager dirtyStateMgr = null)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _projectHelper = projectHelper ?? throw new ArgumentNullException(nameof(projectHelper));
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();

            GenDocType = Model.GenDocType;
            HtmlGenerationProfiles = _projectHelper.GetHtmlGenerationProfiles();
            SelectedHtmlGenerationProfile = Model.CurrentHtmlGenerationProfile;
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
            
            // When GenDocType changes, update the model
            this.WhenAnyValue(x => x.GenDocType)
                .Skip(1) // Skip initial set, only react to user changes
                .Subscribe(type =>
                {
                    if (Model.GenDocType != type) // Update only if changed
                    {
                        Model.GenDocType = type;
                        _dirtyStateMgr.MarkAsDirty();
                    }
                })
                .DisposeWith(_cleanup);
        }

        public void Dispose() => _cleanup.Dispose();

        // IDirtyTrackable implementation
        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;
    }
}
