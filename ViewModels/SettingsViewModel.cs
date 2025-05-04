using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.ViewModels
{
    public sealed class SettingsViewModel : ReactiveObject
    {
        public Settings Model { get; }

        public SettingsViewModel(Settings model) => Model = model;

        // Wrapper property for GenDocType
        public GenerateFileTypeEnum GenDocType
        {
            get => Model.GenDocType;
            set
            {
                if (Model.GenDocType != value)
                {
                    Model.GenDocType = value;
                    this.RaisePropertyChanged();
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
                    this.RaisePropertyChanged();
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
                    GenDocType = GenerateFileTypeEnum.HTML;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsDocxSelected));
                }
            }
        }
    }
}
