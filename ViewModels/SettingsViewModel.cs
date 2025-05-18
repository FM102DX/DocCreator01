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
using DocCreator01.Messages;

namespace DocCreator01.ViewModels
{
    public sealed class SettingsViewModel : ReactiveObject, IDirtyTrackable
    {
        public Settings Model { get; }
        private readonly IDirtyStateManager _dirtyStateMgr;
        private readonly IProjectHelper _projectHelper;
        private readonly CompositeDisposable _cleanup = new CompositeDisposable();
        public Guid Id { get; set; }= Guid.NewGuid(); 

        [Reactive] public IEnumerable<HtmlGenerationProfile> HtmlGenerationProfiles { get; set; }
        [Reactive] public HtmlGenerationProfile? SelectedHtmlGenerationProfile { get; set; }
        [Reactive] public GenerateFileTypeEnum GenDocType { get; set; }

        private bool _suppressBroadcast; 

        public SettingsViewModel(Settings model, IProjectHelper projectHelper, IDirtyStateManager dirtyStateMgr = null)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _projectHelper = projectHelper ?? throw new ArgumentNullException(nameof(projectHelper));
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();

            GenDocType = Model.GenDocType;
            HtmlGenerationProfiles = _projectHelper.GetHtmlGenerationProfiles();
            SelectedHtmlGenerationProfile = Model.CurrentHtmlGenerationProfile;

            //this.SelectedHtmlGenerationProfile
            this.WhenAnyValue(x => x.SelectedHtmlGenerationProfile)
               // .Skip(1) // Skip initial set, only react to user changes
                .Subscribe(profile =>
                {
                    Model.CurrentHtmlGenerationProfile = profile;
                    Model.CurrentHtmlGenerationProfileId = profile?.Id ?? 0;
                    _dirtyStateMgr.MarkAsDirty();
                    Publish(nameof(SelectedHtmlGenerationProfile), profile);
                })
                .DisposeWith(_cleanup);

            //this.GenDocType
            this.WhenAnyValue(x => x.GenDocType)
             //   .Skip(1) // Skip initial set, only react to user changes
                .Subscribe(type =>
                {
                    Model.GenDocType = type;
                    _dirtyStateMgr.MarkAsDirty();
                    Publish(nameof(GenDocType), type);
                })
                .DisposeWith(_cleanup);

            // SUBSCRIBE to other VMs
            MessageBus.Current.Listen<SettingsChangedMessage>()
                .Where(m => !ReferenceEquals(m.Sender, this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(m =>
                {
                    _suppressBroadcast = true;                  // prevent loop
                    switch (m.PropertyName)
                    {
                        case nameof(GenDocType):
                            GenDocType = (GenerateFileTypeEnum)m.Value!;
                            break;
                        case nameof(SelectedHtmlGenerationProfile):
                            SelectedHtmlGenerationProfile = (HtmlGenerationProfile?)m.Value;
                            break;
                    }
                    _suppressBroadcast = false;
                })
                .DisposeWith(_cleanup);

        }

        public void Dispose() => _cleanup.Dispose();

        // IDirtyTrackable implementation
        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;

        private void Publish(string prop, object? val)
        {
            if (_suppressBroadcast) return;                 // don’t echo externally-initiated changes
            MessageBus.Current.SendMessage(
                new SettingsChangedMessage(prop, val, this));
        }

    }
}
