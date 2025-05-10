using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using System.Reactive.Linq;
using DocCreator01.Contracts;
using DocCreator01.Services;

namespace DocCreator01.ViewModels
{
    public sealed class SettingsViewModel : ReactiveObject, IDirtyTrackable
    {
        public Settings Model { get; }
        private readonly IDirtyStateManager _dirtyStateMgr;

        public SettingsViewModel(Settings model, IDirtyStateManager dirtyStateMgr = null)
        {
            Model = model;
            _dirtyStateMgr = dirtyStateMgr ?? new DirtyStateManager();

            // Subscribe to Model's GenDocType changes
            this.WhenAnyValue(x => x.Model.GenDocType)
                .Skip(1) // Skip initial value
                .Subscribe(_ =>
                {
                    _dirtyStateMgr.MarkAsDirty();
                    this.RaisePropertyChanged(nameof(GenDocType));
                    this.RaisePropertyChanged(nameof(IsDocxSelected));
                    this.RaisePropertyChanged(nameof(IsHtmlSelected));
                });
        }

        // Get IsDirty directly from the manager

        // Wrapper property for GenDocType
        public GenerateFileTypeEnum GenDocType
        {
            get => Model.GenDocType;
            set
            {
                if (Model.GenDocType != value)
                {
                    Model.GenDocType = value;
                   // _dirtyStateMgr.MarkIsDirty();
                }
            }
        }

        // Helper properties for radio button bindings
        public bool IsDocxSelected
        {
            get => Model.GenDocType == GenerateFileTypeEnum.DOCX;
            set
            {
                if (value && Model.GenDocType != GenerateFileTypeEnum.DOCX)
                {
                    GenDocType = GenerateFileTypeEnum.DOCX;
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
                    GenDocType = GenerateFileTypeEnum.HTML;
                }
            }
        }

        // IDirtyTrackable implementation
        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;
    }
}
