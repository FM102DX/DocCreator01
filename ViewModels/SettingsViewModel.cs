using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
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
            Model = model;
            _projectHelper = projectHelper;
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();

            // Load HTML Generation Profiles
            HtmlGenerationProfiles = _projectHelper.GetHtmlGenerationProfiles();
            
            // Initialize the selected profile (from model or default)
            SelectedHtmlGenerationProfile = Model.CurrentHtmlGenerationProfile ?? HtmlGenerationProfiles.FirstOrDefault();

            // When SelectedHtmlGenerationProfile changes, update the model
            this.WhenAnyValue(x => x.SelectedHtmlGenerationProfile)
                .Skip(1) // Skip initial value
                .WhereNotNull()
                .Subscribe(profile => 
                {
                    Model.CurrentHtmlGenerationProfile = profile;
                    Model.CurrentHtmlGenerationProfileId = profile.Id;
                    _dirtyStateMgr.MarkAsDirty();
                });

            // Subscribe to model GenDocType changes
            this.WhenAnyValue(x => x.Model.GenDocType)
                .Skip(1) // Skip initial value
                .Subscribe(_ =>
                {
                    _dirtyStateMgr.MarkAsDirty();
                    this.RaisePropertyChanged(nameof(IsDocxSelected));
                    this.RaisePropertyChanged(nameof(IsHtmlSelected));
                });
        }

        // IDirtyTrackable implementation
        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;
    }
}
