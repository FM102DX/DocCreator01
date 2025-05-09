using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Windows;
using Microsoft.Win32;
using DocCreator01.Contracts;
using DocCreator01.Models;
using DocCreator01.Data.Enums;
using DocCreator01.ViewModels;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Controls;
using DocCreator01.Services;

namespace DocCreator01.ViewModel
{
    public sealed class MainWindowViewModel : ReactiveObject
    {
        private readonly IProjectRepository _repo;
        private readonly ITextPartHelper _textPartHelper;
        private readonly IProjectHelper _projectHelper;
        private readonly IAppPathsHelper _appPathsHelper;
        private readonly IGeneratedFilesHelper _generatedFilesHelper;
        private readonly IBrowserService _browserService;
        private string? _currentPath;
        private bool _isProjectDirty;
        private ITabViewModel? _selectedTab;
        private TextPart? _selectedMainGridItem;
        private MainGridItemViewModel? _selectedMainGridItemViewModel;

        public MainWindowViewModel(
            IProjectRepository repo, 
            ITextPartHelper textPartHelper,
            IProjectHelper projectHelper,
            IAppPathsHelper appPathsHelper,
            IGeneratedFilesHelper generatedFilesHelper,
            IBrowserService browserService)
        {
            _repo = repo;
            _textPartHelper = textPartHelper;
            _projectHelper = projectHelper;
            _appPathsHelper = appPathsHelper;
            _generatedFilesHelper = generatedFilesHelper;
            _browserService = browserService;

            // Initialize SettingsViewModel
            SettingsViewModel = new SettingsViewModel(_projectHelper.CurrentProject.Settings);

            // Subscribe to SettingsViewModel IsDirty property
            this.WhenAnyValue(x => x.SettingsViewModel.IsDirty)
                .Where(isDirty => isDirty)
                .Subscribe(_ => IsProjectDirty = true);

            // Subscribe to project changes
            _projectHelper.ProjectChanged += (s, project) => 
            {
                // Update UI when project changes
                _currentPath = project.FilePath;
                SettingsViewModel = new SettingsViewModel(project.Settings);
                this.RaisePropertyChanged(nameof(SettingsViewModel));
                this.RaisePropertyChanged(nameof(CurrentProject));
                RefreshTextPartViewModels();
                UpdateWindowTitle();
                IsProjectDirty = false;
            };

            OpenCommand = ReactiveCommand.Create(OpenFile, outputScheduler: Ui);
            SaveCommand = ReactiveCommand.Create(Save, outputScheduler: Ui);
            ExitCommand = ReactiveCommand.Create(() => Application.Current.Shutdown(), outputScheduler: Ui);
            AddTabCommand = ReactiveCommand.Create(AddTab, outputScheduler: RxApp.MainThreadScheduler);
            CloseTabCommand = ReactiveCommand.Create<ITabViewModel>(vm => CloseTab(vm), outputScheduler: Ui);
            CloseAllTabsCommand = ReactiveCommand.Create(CloseAllTabs, outputScheduler: Ui);
            DeleteTabCommand = ReactiveCommand.Create<ITabViewModel>(DeleteTab, outputScheduler: Ui);
            GenerateCommand = ReactiveCommand.Create(GenerateOutputFile, outputScheduler: Ui);
            AddTextPartCommand = AddTabCommand;
            RemoveTextPartCommand = ReactiveCommand.Create(RemoveCurrent, outputScheduler: Ui);
            MoveUpCommand = ReactiveCommand.Create(MoveCurrentUp, outputScheduler: Ui);
            MoveDownCommand = ReactiveCommand.Create(MoveCurrentDown, outputScheduler: Ui);
            ActivateTabCommand = ReactiveCommand.Create(ActivateTab, outputScheduler: Ui);
            CloseCurrentCommand = ReactiveCommand.Create(CloseCurrent, outputScheduler: Ui);
            OpenRecentCommand = ReactiveCommand.Create<string>(OpenRecent, outputScheduler: Ui);
            MoveLeftCommand = ReactiveCommand.Create(MoveCurrentLeft, outputScheduler: Ui);
            MoveRightCommand = ReactiveCommand.Create(MoveCurrentRight, outputScheduler: Ui);
            OpenDocumentsFolderCommand = ReactiveCommand.Create(() => OpenFolder(_appPathsHelper.DocumentsOutputDirectory));
            OpenScriptsFolderCommand = ReactiveCommand.Create(() => OpenFolder(_appPathsHelper.ScriptsDirectory));
            OpenProjectFolderCommand = ReactiveCommand.Create(() => OpenFolder(CurrentProject.ProjectFolder));
            OpenSettingsTabCommand = ReactiveCommand.Create(OpenSettingsTab, outputScheduler: Ui);

            // Обновление строк главного грида
            this.WhenAnyValue(x => x.CurrentProject)
                .Subscribe(_ => RefreshMainGridItemsViewModels(CurrentProject.ProjectData.TextParts, MainGridLines));

            //пере-инициализация _generatedFilesHelper
            this.WhenAnyValue(x => x.CurrentProject)
                .Subscribe(_ => _generatedFilesHelper.Initialize(CurrentProject));

            this.WhenAnyValue(x => x.CurrentProject.ProjectData.GeneratedFiles)
                .Subscribe(_ => RefreshGeneratedFilesViewModels(CurrentProject.ProjectData.GeneratedFiles, GeneratedFilesViewModels));
            

            LoadRecentFiles(); // Load recent files on startup
        }

        #region commands

        // Collection of TextPartListViewModels for display in the DataGrid
        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTabCommand { get; }
        public ReactiveCommand<ITabViewModel, Unit> CloseTabCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseAllTabsCommand { get; }
        public ReactiveCommand<ITabViewModel, Unit> DeleteTabCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateCommand { get; }
        public ReactiveCommand<string, Unit> OpenRecentCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTextPartCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveTextPartCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveUpCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveDownCommand { get; }
        public ReactiveCommand<Unit, Unit> ActivateTabCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCurrentCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveLeftCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveRightCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenDocumentsFolderCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenScriptsFolderCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenProjectFolderCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsTabCommand { get; }

        #endregion

        public ObservableCollection<MainGridItemViewModel> MainGridLines { get; } = new();
        public bool IsProjectDirty
        {
            get => _isProjectDirty;
            private set
            {
                this.RaiseAndSetIfChanged(ref _isProjectDirty, value);
                UpdateWindowTitle(); // сразу обновляем заголовок
            }
        }
        
        public Project CurrentProject => _projectHelper.CurrentProject;

        public MainGridItemViewModel? SelectedMainGridItemViewModel
        {
            get => _selectedMainGridItemViewModel;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedMainGridItemViewModel, value);
                if (value != null)
                    SelectedMainGridItem = value.Model;
                else
                    SelectedMainGridItem = null;
            }
        }
        
        public ObservableCollection<string> RecentFiles { get; } = new();
        public ObservableCollection<ITabViewModel> Tabs { get; } = new();
        public ObservableCollection<GeneratedFileViewModel> GeneratedFilesViewModels { get; } = new();
        
        public ITabViewModel? SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }
        
        public TextPart? SelectedMainGridItem
        {
            get => _selectedMainGridItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedMainGridItem, value);
                if (_selectedMainGridItemViewModel == null ||
                    (_selectedMainGridItem != null && _selectedMainGridItemViewModel.Model != _selectedMainGridItem))
                {
                    _selectedMainGridItemViewModel =
                        MainGridLines.FirstOrDefault(vm => vm.Model == _selectedMainGridItem);
                    this.RaisePropertyChanged(nameof(SelectedMainGridItemViewModel));
                }
            }
        }

        public string WindowTitle =>
            string.IsNullOrEmpty(_currentPath)
                ? $"{CurrentProject.Name}{(IsProjectDirty ? " *" : "")} [not-saved]"
                : $"{CurrentProject.Name} - {Path.GetFileName(_currentPath)}{(IsProjectDirty ? " *" : "")}";

        private void UpdateWindowTitle() => this.RaisePropertyChanged(nameof(WindowTitle));

        private void SubscribeTab(ITabViewModel vm)
        {
            // любое IsDirty=true на вкладке делает «грязным» весь проект
            if (vm is ReactiveObject reactiveVm)
            {
                try
                {
                    // Add exception handling to the subscription
                    reactiveVm.WhenAnyValue(x => ((ITabViewModel)x).IsDirty)
                        .Where(d => d)                     // интересует только переход в true
                        .Subscribe(_ => IsProjectDirty = true,
                        // Add error handler to prevent unhandled exceptions in subscription
                        ex => 
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in IsDirty subscription for tab: {ex.Message}");
                            // Optionally log or handle the error
                        });
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during subscription setup
                    System.Diagnostics.Debug.WriteLine($"Error setting up tab subscription: {ex.Message}");
                }
            }
        }

        private IScheduler Ui => RxApp.MainThreadScheduler;

        private void RefreshTextPartViewModels()
        {
            //RefreshTextPartViewModels(CurrentProject.ProjectData.TextParts, MainGridLines);
        }

        private void AddTab()
        {
            var tp = _textPartHelper.CreateTextPart(CurrentProject);
            CurrentProject.ProjectData.TextParts.Add(tp);

            var vm = new TabPageViewModel(tp);
            SubscribeTab(vm);
            Tabs.Add(vm);
            SelectedTab = vm;

            IsProjectDirty = true;          // structure changed
        }

        private void CloseTab(ITabViewModel? vm) => Tabs.Remove(vm!);
        private void CloseAllTabs() => Tabs.Clear();

        private void DeleteTab(ITabViewModel? vm)
        {
            if (vm == null) return;

            // Only handle TextPart tabs
            if (vm is TabPageViewModel textPartVm)
            {
                _textPartHelper.RemoveTextPart(textPartVm.TextPart, CurrentProject.ProjectData.TextParts, MainGridLines);
                CloseTab(vm);
                IsProjectDirty = true;
            }
        }

        private void OpenFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Doc Parts (*.docparts)|*.docparts|All (*.*)|*.*",
                DefaultExt = ".docparts"
            };
            if (dlg.ShowDialog() != true) return;
            LoadProject(dlg.FileName);
        }

        private void OpenRecent(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show($"Файл «{fileName}» не найден.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                RecentFiles.Remove(fileName);      // удаляем битую запись
                return;
            }
            LoadProject(fileName);
        }

        private void LoadProject(string fileName)
        {
            // Clear old state
            Tabs.Clear();
            MainGridLines.Clear();
            GeneratedFilesViewModels.Clear();

            // Use ProjectHelper to load project
            _projectHelper.LoadProject(fileName);
            _currentPath = fileName;

            // Load generated files
            LoadGeneratedFiles();

            // Add recent files entry
            AddRecent(fileName);
            
            // Create tabs for opened tabs in project
            foreach (var tp in CurrentProject.ProjectData.TextParts)
            {
                if (CurrentProject.OpenedTabs.Contains(tp.Id))
                {
                    var vm = new TabPageViewModel(tp);
                    SubscribeTab(vm);
                    Tabs.Add(vm);
                }
            }
            
            SelectedTab = Tabs.FirstOrDefault();
            IsProjectDirty = false;
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(_currentPath))
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "Doc Parts (*.docparts)|*.docparts",
                    DefaultExt = ".docparts",
                    FileName = $"{CurrentProject.Name}.docparts" // Set default filename to project name
                };
                if (dlg.ShowDialog() == true)
                {
                    _currentPath = dlg.FileName;
                    CurrentProject.FilePath = _currentPath;
                    UpdateWindowTitle();
                }
                else return;
            }
            
            CurrentProject.OpenedTabs = Tabs.OfType<TabPageViewModel>()
                .Where(t => t.TextPart != null)
                .Select(x => x.TextPart.Id)
                .ToList();
            
            // Use ProjectHelper to save project
            _projectHelper.SaveProject(CurrentProject, _currentPath!);
            AddRecent(_currentPath);
            
            // Reset dirty flags for all tabs
            foreach (var tab in Tabs)
                tab.AcceptChanges();

            SettingsViewModel.AcceptChanges();
            IsProjectDirty = false;
        }

        private async void GenerateOutputFile()
        {
            try
            {
                // Determine which file type to generate based on settings
                GenerateFileTypeEnum fileType = SettingsViewModel.IsHtmlSelected 
                    ? GenerateFileTypeEnum.HTML 
                    : GenerateFileTypeEnum.DOCX;

                _generatedFilesHelper.GenerateFileAsync(fileType);

               
                // Mark project as dirty since we added a generated file
                IsProjectDirty = true;
                
                // Optional: Open the file immediately
                //_generatedFilesHelper.OpenFile(generatedFile);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating file: {ex.Message}", 
                    "Generation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveCurrent()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.TextPart == textPart);

            if (tab != null)
                Tabs.Remove(tab);

            _textPartHelper.RemoveTextPart(textPart, CurrentProject.ProjectData.TextParts, MainGridLines);

            SelectedMainGridItemViewModel = MainGridLines.FirstOrDefault();
            SelectedTab = Tabs.FirstOrDefault();

            IsProjectDirty = true;
        }

        private void MoveCurrentUp()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.MoveTextPartUp(textPart, CurrentProject.ProjectData.TextParts, MainGridLines))
            {
                this.RaisePropertyChanged(nameof(CurrentProject));
                IsProjectDirty = true;

                // Find and mark the affected tab as dirty
                var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.TextPart == textPart);
                if (tab != null)
                {
                    // This will raise the IsDirty property and update the Header
                    tab.MarkAsDirty();
                }

                // Restore selection after moving
                SelectedMainGridItem = textPart;
            }
        }

        private void MoveCurrentDown()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.MoveTextPartDown(textPart, CurrentProject.ProjectData.TextParts, MainGridLines))
            {
                this.RaisePropertyChanged(nameof(CurrentProject));
                IsProjectDirty = true;

                // Find and mark the affected tab as dirty
                var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.TextPart == textPart);
                if (tab != null)
                {
                    // This will raise the IsDirty property and update the Header
                    tab.MarkAsDirty();
                }

                // Restore selection after moving
                SelectedMainGridItem = textPart;
            }
        }

        private void MoveCurrentLeft()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.DecreaseTextPartLevel(textPart))
            {
                IsProjectDirty = true;
            }
        }

        private void MoveCurrentRight()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.IncreaseTextPartLevel(textPart))
            {
                IsProjectDirty = true;
            }
        }

        private void ActivateTab()
        {
            var tp = SelectedMainGridItem;
            var vm = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.TextPart == tp);
            if (vm == null)
            {
                vm = new TabPageViewModel(tp);
                Tabs.Add(vm);
            }
            SelectedTab = vm;
        }

        private void CloseCurrent()
        {
            // Delegate to ProjectHelper to handle closing
            if (_projectHelper.CloseCurrentProject(IsProjectDirty ? null : false))
            {
                // ProjectHelper succeeded in closing the project
                // Clear UI state
                Tabs.Clear();
                GeneratedFilesViewModels.Clear();
                MainGridLines.Clear();
                SelectedTab = null;
                SelectedMainGridItemViewModel = null;
                
                // Current project has already been updated by ProjectHelper
                _currentPath = CurrentProject.FilePath;
            }
        }

        private void LoadRecentFiles()
        {
            if (File.Exists(_appPathsHelper.SettingsFilePath))
            {
                var recentFiles = File.ReadAllLines(_appPathsHelper.SettingsFilePath);
                foreach (var file in recentFiles)
                {
                    if (!string.IsNullOrWhiteSpace(file))
                        RecentFiles.Add(file);
                }
            }
        }

        private void SaveRecentFiles()
        {
            File.WriteAllLines(_appPathsHelper.SettingsFilePath, RecentFiles);
        }

        private void AddRecent(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            // дубликат? — поднимаем вверх
            if (RecentFiles.Contains(path))
                RecentFiles.Remove(path);

            RecentFiles.Insert(0, path);

            while (RecentFiles.Count > 5)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);

            SaveRecentFiles(); // Save changes to the file
        }

        private void LoadGeneratedFiles()
        {
            
        }

        private void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                });
            }
            else
            {
                MessageBox.Show($"Folder not found: {folderPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSettingsTab()
        {
            // Check if settings tab is already open
            var existingSettingsTab = Tabs.OfType<ProjectSettingsTabViewModel>().FirstOrDefault();
            
            if (existingSettingsTab != null)
            {
                // If settings tab is already open, just select it
                SelectedTab = existingSettingsTab;
            }
            else
            {
                // Create a new settings tab
                var settingsTab = new ProjectSettingsTabViewModel(CurrentProject.Settings);
                SubscribeTab(settingsTab);
                Tabs.Add(settingsTab);
                SelectedTab = settingsTab;
            }
        }

        // Add this property to MainWindowViewModel class
        public SettingsViewModel SettingsViewModel { get; private set; }
        public void RefreshMainGridItemsViewModels(ObservableCollection<TextPart> textParts, ObservableCollection<MainGridItemViewModel> viewModels)
        {
            viewModels.Clear();

            foreach (var textPart in textParts)
            {
                viewModels.Add(new MainGridItemViewModel(textPart));
            }

            // Ensure we stay synchronized with the model collection
            textParts.CollectionChanged += (s, e) =>
            {
                // Re-build the view models collection when the underlying collection changes
                viewModels.Clear();
                foreach (var textPart in textParts)
                {
                    viewModels.Add(new MainGridItemViewModel(textPart));
                }
            };
        }

        public void RefreshGeneratedFilesViewModels(ObservableCollection<GeneratedFile> sourceItems, ObservableCollection<GeneratedFileViewModel> viewModels)
        {
            viewModels.Clear();

            foreach (var item in sourceItems)
            {
                // Make sure Project reference is set
                item.Project = CurrentProject;
                
                // Pass all required dependencies to the view model
                viewModels.Add(new GeneratedFileViewModel(
                    item, 
                    _appPathsHelper,
                    _generatedFilesHelper, 
                    _browserService));
            }

            // Ensure we stay synchronized with the model collection
            sourceItems.CollectionChanged += (s, e) =>
            {
                // Re-build the view models collection when the underlying collection changes
                viewModels.Clear();
                foreach (var item in sourceItems)
                {
                    // Make sure Project reference is set
                    item.Project = CurrentProject;
                    
                    // Pass all required dependencies to the view model
                    viewModels.Add(new GeneratedFileViewModel(
                        item, 
                        _appPathsHelper,
                        _generatedFilesHelper, 
                        _browserService));
                }
            };
        }
    }
}
