using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using ReactiveUI;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Reactive;

namespace DocCreator01.ViewModels
{
    /// <summary>
    /// View model for a generated file that includes UI-specific properties like icons
    /// </summary>
    public class GeneratedFileViewModel : ReactiveObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IAppPathsHelper _appPathsHelper;
        private readonly IGeneratedFilesHelper _generatedFilesHelper;
        private readonly IBrowserService _browserService;
        private readonly GeneratedFile _model;
        private const int MaxFileNameLength = 20; // Maximum length for filename before truncation

        public GeneratedFileViewModel(
            GeneratedFile model, 
            IAppPathsHelper appPathsHelper,
            IGeneratedFilesHelper generatedFilesHelper,
            IBrowserService browserService)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _appPathsHelper = appPathsHelper ?? throw new ArgumentNullException(nameof(appPathsHelper));
            _generatedFilesHelper = generatedFilesHelper ?? throw new ArgumentNullException(nameof(generatedFilesHelper));
            _browserService = browserService ?? throw new ArgumentNullException(nameof(browserService));

            // Initialize commands
            DeleteCommand = ReactiveCommand.Create(DeleteFile);
            RenameCommand = ReactiveCommand.Create(RenameFile);
            OpenInBrowserCommand = ReactiveCommand.Create(OpenInBrowser);
            OpenInNotepadPlusPlusCommand = ReactiveCommand.Create(OpenInNotepadPlusPlus);
            OpenInWordCommand = ReactiveCommand.Create(OpenInWord);
        }

        #region Commands
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> RenameCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenInBrowserCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenInNotepadPlusPlusCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenInWordCommand { get; }
        #endregion

        #region Command Implementations
        private void DeleteFile()
        {
            // Only delete if we have access to the helper
            if (_generatedFilesHelper != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить файл \"{FileName}\"?",
                    "Удаление файла",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Get access to the parent collection
                        var projectDataCollection = _model.Project?.ProjectData?.GeneratedFiles;
                        if (projectDataCollection != null)
                        {
                            _generatedFilesHelper.DeleteFile(_model, projectDataCollection);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Не удалось удалить файл - проблема с коллекцией файлов.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Ошибка при удалении файла: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RenameFile()
        {
            // Stub implementation - to be expanded in the future
            MessageBox.Show(
                "Функция переименования файла будет реализована в следующей версии.",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OpenInBrowser()
        {
            if (_browserService != null && Exists)
            {
                try
                {
                    // Use the shell to open with default browser
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Не удалось открыть файл в браузере: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OpenInNotepadPlusPlus()
        {
            if (_browserService != null && Exists)
            {
                if (!_browserService.OpenInNotepadPlusPlus(FilePath))
                {
                    MessageBox.Show(
                        "Не удалось открыть файл в Notepad++.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OpenInWord()
        {
            if (Exists)
            {
                try
                {
                    // Use the shell to open with default application (Word for .docx)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Не удалось открыть файл в Word: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Properties
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

        /// <summary>
        /// Determines if the file is an HTML file
        /// </summary>
        public bool IsHtmlFile => _model.FileType == GenerateFileTypeEnum.HTML;

        /// <summary>
        /// Determines if the file is a DOCX file
        /// </summary>
        public bool IsDocxFile => _model.FileType == GenerateFileTypeEnum.DOCX;

        /// <summary>
        /// Determines if the file can be opened in Notepad++
        /// </summary>
        public bool IsNotepadCompatibleFile => _model.FileType == GenerateFileTypeEnum.HTML || 
                                               Extension.Equals("txt", StringComparison.OrdinalIgnoreCase);
        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
