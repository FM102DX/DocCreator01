using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.ViewModels
{
    public sealed class SettingsViewModel : ReactiveObject
    {
        public Settings Model { get; }
        private bool _isDirty;

        public SettingsViewModel(Settings model)
        {
            Model = model;

            // Subscribe to Model's GenDocType changes
            this.WhenAnyValue(x => x.Model.GenDocType)
                .Skip(1) // Skip initial value
                .Subscribe(_ =>
                {
                    IsDirty = true;
                    this.RaisePropertyChanged(nameof(GenDocType));
                    this.RaisePropertyChanged(nameof(IsDocxSelected));
                    this.RaisePropertyChanged(nameof(IsHtmlSelected));
                });
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set => this.RaiseAndSetIfChanged(ref _isDirty, value);
        }

        // Wrapper property for GenDocType
        public GenerateFileTypeEnum GenDocType
        {
            get => Model.GenDocType;
            set
            {
                if (Model.GenDocType != value)
                {
                    Model.GenDocType = value;
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

        /// <summary>Resets the dirty flag after project is saved</summary>
        public void AcceptChanges()
        {
            IsDirty = false;
        }
    }
}
