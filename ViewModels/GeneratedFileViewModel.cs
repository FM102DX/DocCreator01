using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DocCreator01.ViewModels
{
    /// <summary>
    /// View model for a generated file that includes UI-specific properties like icons
    /// </summary>
    public class GeneratedFileViewModel : ReactiveObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IAppPathsHelper _appPathsHelper;
        private readonly GeneratedFile _model;
        private const int MaxFileNameLength = 20; // Maximum length for filename before truncation

        public GeneratedFileViewModel(GeneratedFile model, IAppPathsHelper appPathsHelper)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _appPathsHelper = appPathsHelper ?? throw new ArgumentNullException(nameof(appPathsHelper));
        }

        /// <summary>
        /// The underlying generated file model
        /// </summary>
        public GeneratedFile Model => _model;

        /// <summary>
        /// Gets the file name from the full path
        /// </summary>
        public string FileName => _model.FileName;

        /// <summary>
        /// Gets the truncated file name for display with ellipsis if needed
        /// </summary>
        public string DisplayFileName
        {
            get
            {
                string fileName = _model.FileName;
                if (fileName.Length <= MaxFileNameLength)
                    return fileName;

                // Get file extension
                string extension = Path.GetExtension(fileName);
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                
                // Keep the first part and add ellipsis
                int charsToKeep = MaxFileNameLength - 3 - extension.Length;
                if (charsToKeep < 1) charsToKeep = 1; // At least keep 1 char
                
                return nameWithoutExtension.Substring(0, charsToKeep) + "..." + extension;
            }
        }

        /// <summary>
        /// Gets the full path to the file
        /// </summary>
        public string FilePath => _model.FilePath;

        /// <summary>
        /// Gets the file extension without the dot
        /// </summary>
        public string Extension => _model.Extension;

        /// <summary>
        /// Gets the path to the icon for this file type
        /// </summary>
        public string IconPath
        {
            get
            {
                string iconFileName = _model.FileType switch
                {
                    GenerateFileTypeEnum.HTML => "HtmlIcon.jpg",
                    GenerateFileTypeEnum.DOCX => "DocxIcon.jpg",
                    _ => "TxtIcon.jpg" // Default icon
                };

                // Fix: Use proper resource URI format for WPF resources
                return $"/Resources/Icons/{iconFileName}";
            }
        }

        /// <summary>
        /// Determines if the file exists on disk
        /// </summary>
        public bool Exists => _model.Exists;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
