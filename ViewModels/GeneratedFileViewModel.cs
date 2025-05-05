using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using System;
using System.IO;

namespace DocCreator01.ViewModels
{
    /// <summary>
    /// View model for a generated file that includes UI-specific properties like icons
    /// </summary>
    public class GeneratedFileViewModel : ReactiveObject
    {
        private readonly IAppPathsHelper _appPathsHelper;
        private readonly GeneratedFile _model;

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
                    GenerateFileTypeEnum.HTML => "html_icon.png",
                    GenerateFileTypeEnum.DOCX => "docx_icon.png",
                    _ => "document_icon.png" // Default icon
                };

                return Path.Combine(_appPathsHelper.IconsDirectory, iconFileName);
            }
        }

        /// <summary>
        /// Determines if the file exists on disk
        /// </summary>
        public bool Exists => _model.Exists;
    }
}
