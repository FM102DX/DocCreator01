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
using System.ComponentModel;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;
using DocCreator01.Messages;

namespace DocCreator01.ViewModel
{
    public sealed class MainWindowViewModel : ReactiveObject, IDirtyTrackable, IDisposable
    {
        private readonly IProjectRepository _repo;
        private readonly ITextPartHelper _textPartHelper;
        private readonly IProjectHelper _projectHelper;
        private readonly IAppPathsHelper _appPathsHelper;
        private readonly IGeneratedFilesHelper _generatedFilesHelper;
        private readonly IBrowserService _browserService;
        private readonly IDirtyStateManager _dirtyStateMgr;
        private readonly CompositeDisposable _cleanup = new();

        private string? _currentPath;
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
            _dirtyStateMgr = new DirtyStateManager();
            
            // Subscribe to project changes
            _projectHelper.ProjectChanged += (s, project) => 
            {
                // Update UI when project changes
                RefreshTextPartViewModels();
                this.RaisePropertyChanged(nameof(CurrentProject));
                this.RaisePropertyChanged(nameof(WindowTitle));

                // Reset dirty state when project changes
                _dirtyStateMgr.ResetDirtyState();
            };

            // Subscribe to dirty state for window title update
            _dirtyStateMgr.IBecameDirty += () => 
                this.RaisePropertyChanged(nameof(WindowTitle));
            _dirtyStateMgr.DirtryStateWasReset += () =>
                this.RaisePropertyChanged(nameof(WindowTitle));

            // Commands
            NewProjectCommand = ReactiveCommand.Create(CreateNewProjectUi);
            AddTabCommand = ReactiveCommand.Create(AddTab);
            OpenCommand = ReactiveCommand.Create(OpenFile);
            OpenRecentCommand = ReactiveCommand.Create<string>(OpenRecent);
            SaveCommand = ReactiveCommand.Create(Save);
            CloseTabCommand = ReactiveCommand.Create<ITabViewModel>(CloseTab);
            DeleteTabCommand = ReactiveCommand.Create<ITabViewModel>(DeleteTab);
            ExitCommand = ReactiveCommand.Create(() => Application.Current.Shutdown());
            AddTextPartCommand = ReactiveCommand.Create(AddTab);
            RemoveTextPartCommand = ReactiveCommand.Create(RemoveCurrent);
            MoveUpCommand = ReactiveCommand.Create(MoveCurrentUp);
            MoveDownCommand = ReactiveCommand.Create(MoveCurrentDown);
            CloseCurrentCommand = ReactiveCommand.Create(CloseCurrent);
            MoveLeftCommand = ReactiveCommand.Create(MoveCurrentLeft);
            MoveRightCommand = ReactiveCommand.Create(MoveCurrentRight);
            ActivateTabCommand = ReactiveCommand.Create(ActivateTab);
            OpenSettingsTabCommand = ReactiveCommand.Create(OpenSettingsTab);
            
            OpenDocumentsFolderCommand = ReactiveCommand.Create(() => OpenFolder(_appPathsHelper.DocumentsOutputDirectory));
            OpenScriptsFolderCommand = ReactiveCommand.Create(() => OpenFolder(_appPathsHelper.ScriptsDirectory));
            OpenProjectFolderCommand = ReactiveCommand.Create(() => {
                if (!string.IsNullOrEmpty(CurrentProject.ProjectFolder))
                    OpenFolder(CurrentProject.ProjectFolder);
            });

            GenerateFileCommand = ReactiveCommand.Create(GenerateOutputFile);

            _projectHelper.CurrentProject.ProjectData.TextParts.CollectionChanged += (s, e) =>
                RefreshTextPartViewModels();

            _generatedFilesHelper.Initialize(CurrentProject);

            //this.WhenAnyValue(x => x.CurrentProject.ProjectData.GeneratedFiles)
            //    .Subscribe(_ =>
            //    {
            //        RefreshGeneratedFilesViewModels(CurrentProject.ProjectData.GeneratedFiles,
            //            GeneratedFilesViewModels);
            //    });
            
            LoadRecentFiles(); // Load recent files on startup

            // React when generated files are updated
            MessageBus.Current
                .Listen<GeneratedFilesUpdatedMessage>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                    RefreshGeneratedFilesViewModels(
                        CurrentProject.ProjectData.GeneratedFiles,
                        GeneratedFilesViewModels))
                .DisposeWith(_cleanup);

            // React when project is loaded
            MessageBus.Current
                .Listen<ProjectLoadedMessage>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(msg => {
                    // Handle any necessary UI updates after project is loaded
                    RefreshGeneratedFilesViewModels(
                        msg.Project.ProjectData.GeneratedFiles,
                        GeneratedFilesViewModels);
                })
                .DisposeWith(_cleanup);
        }

        #region commands

        // Collection of TextPartListViewModels for display in the DataGrid
        public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTabCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<string, Unit> OpenRecentCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<ITabViewModel, Unit> CloseTabCommand { get; }
        public ReactiveCommand<ITabViewModel, Unit> DeleteTabCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateFileCommand { get; }
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
        public bool IsProjectDirty => DirtyStateMgr.IsDirty;

        [Reactive] public SettingsViewModel SettingsViewModel { get; private set; }

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
                _selectedMainGridItemViewModel = MainGridLines.FirstOrDefault(vm => vm.Model == _selectedMainGridItem);
                this.RaisePropertyChanged(nameof(SelectedMainGridItemViewModel));
            }
        }

        public string WindowTitle =>
            string.IsNullOrEmpty(_currentPath)
                ? $"{CurrentProject.Name}{(IsProjectDirty ? " *" : "")} [not-saved]"
                : $"{CurrentProject.Name} - {Path.GetFileName(_currentPath)}{(IsProjectDirty ? " *" : "")}";

        private void UpdateWindowTitle() => this.RaisePropertyChanged(nameof(WindowTitle));

        private void SubscribeTab(ITabViewModel vm)
        {
           if (vm is IDirtyTrackable dirtyTrackable)
           {
               _dirtyStateMgr.AddSubscription(dirtyTrackable);
           }
        }

        private IScheduler Ui => RxApp.MainThreadScheduler;

        private void RefreshTextPartViewModels()
        {
            RefreshTextPartViewModels(CurrentProject.ProjectData.TextParts, MainGridLines);
        }

        private void RefreshTextPartViewModels(
            ObservableCollection<TextPart> models,
            ObservableCollection<MainGridItemViewModel> viewModels)
        {

            if (models == null) return;
            viewModels.Clear();
            NumerationHelper.ApplyNumeration(models);
            foreach (var model in models)
            {
                var vm = new MainGridItemViewModel(model);
                viewModels.Add(vm);
            }

            
        }

        private void AddTab()
        {
            var tp = _textPartHelper.CreateTextPart(CurrentProject);
            CurrentProject.ProjectData.TextParts.Add(tp);
            var vm = new TabPageViewModel(tp, new DirtyStateManager());
            SubscribeTab(vm);
            Tabs.Add(vm);
            SelectedTab = vm;
            _dirtyStateMgr.MarkAsDirty();
            RefreshTextPartViewModels();
        }

        private void CloseTab(ITabViewModel? vm) => Tabs.Remove(vm!);
        private void CloseAllTabs() => Tabs.Clear();

        private void DeleteTab(ITabViewModel? vm)
        {
            if (vm == null) return;

            // Only handle TextPart tabs
            if (vm is TabPageViewModel textPartVm)
            {
                _textPartHelper.RemoveTextPart(textPartVm.Model, CurrentProject.ProjectData.TextParts, MainGridLines);
                CloseTab(vm);
                _dirtyStateMgr.MarkAsDirty();
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
                    var dirtyMgr = new DirtyStateManager();
                    var vm = new TabPageViewModel(tp, dirtyMgr);
                    SubscribeTab(vm);
                    Tabs.Add(vm);
                }
            }
            SettingsViewModel = new SettingsViewModel(_projectHelper.CurrentProject.Settings, _projectHelper, new DirtyStateManager());
            _dirtyStateMgr.AddSubscription(SettingsViewModel);
            SelectedTab = Tabs.FirstOrDefault();
            _generatedFilesHelper.Initialize(CurrentProject);
            _dirtyStateMgr.ResetDirtyState();
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
                .Select(x => x.Model.Id)
                .ToList();
            
            // Use ProjectHelper to save project
            _projectHelper.SaveProject(CurrentProject, _currentPath!);
            AddRecent(_currentPath);
            
            // Accept all changes at once
            _dirtyStateMgr.ResetDirtyState();
        }

        private async void GenerateOutputFile()
        {
            try
            {
                await _generatedFilesHelper.GenerateFileAsync(CurrentProject.Settings.GenDocType);
                RefreshGeneratedFilesViewModels(CurrentProject.ProjectData.GeneratedFiles, GeneratedFilesViewModels);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error generating file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshGeneratedFilesViewModels(
            ObservableCollection<GeneratedFile> models, 
            ObservableCollection<GeneratedFileViewModel> viewModels)
        {
            viewModels.Clear();

            if (models == null) return;

            foreach (var model in models.Where(m => m.Exists))
            {
                var vm = new GeneratedFileViewModel(model, _appPathsHelper, _generatedFilesHelper, _browserService);
                viewModels.Add(vm);
            }
        }

        private void RemoveCurrent()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == textPart);

            if (tab != null)
                Tabs.Remove(tab);

            _textPartHelper.RemoveTextPart(textPart, CurrentProject.ProjectData.TextParts, MainGridLines);

            SelectedMainGridItemViewModel = MainGridLines.FirstOrDefault();
            SelectedTab = Tabs.FirstOrDefault();

            _dirtyStateMgr.MarkAsDirty();
        }

        private void MoveCurrentUp()
        {
            if (SelectedMainGridItemViewModel == null) return;
            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.MoveTextPartUp(textPart, CurrentProject.ProjectData.TextParts, MainGridLines))
            {
                this.RaisePropertyChanged(nameof(CurrentProject));
                _dirtyStateMgr.MarkAsDirty();
                var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == textPart);
                if (tab != null && tab is IDirtyTrackable dtr)
                {
                    dtr.DirtyStateMgr.MarkAsDirty();
                }
                RefreshTextPartViewModels();
            }
            SelectedMainGridItem = textPart;
        }

        private void MoveCurrentDown()
        {
            if (SelectedMainGridItemViewModel == null) return;
            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.MoveTextPartDown(textPart, CurrentProject.ProjectData.TextParts, MainGridLines))
            {
                this.RaisePropertyChanged(nameof(CurrentProject));
                _dirtyStateMgr.MarkAsDirty();
                var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == textPart);
                if (tab != null && tab is IDirtyTrackable dtr)
                {
                    dtr.DirtyStateMgr.MarkAsDirty();
                }
                RefreshTextPartViewModels();
            }
            SelectedMainGridItem = textPart;
        }

        private void MoveCurrentLeft()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.DecreaseTextPartLevel(textPart))
            {
                _dirtyStateMgr.MarkAsDirty();
                RefreshTextPartViewModels();
            }
            SelectedMainGridItem = textPart;
        }

        private void MoveCurrentRight()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (_textPartHelper.IncreaseTextPartLevel(textPart))
            {
                _dirtyStateMgr.MarkAsDirty();
                RefreshTextPartViewModels();
            }
            SelectedMainGridItem = textPart;
        }

        private void ActivateTab()
        {
            var tp = SelectedMainGridItem;
            var vm = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == tp);
            if (vm == null)
            {
                var dirtyMgr = new DirtyStateManager();
                vm = new TabPageViewModel(tp, dirtyMgr);
                SubscribeTab(vm);
                Tabs.Add(vm);
            }
            SelectedTab = vm;
        }

        private void OpenSettingsTab()
        {
            var existingTab = Tabs.FirstOrDefault(t => t is ProjectSettingsTabViewModel);
            
            if (existingTab == null)
            {
                var dirtyMgr = new DirtyStateManager();
                var settingsTab = new ProjectSettingsTabViewModel(CurrentProject, dirtyMgr, _projectHelper);
                SubscribeTab(settingsTab);
                Tabs.Add(settingsTab);
                SelectedTab = settingsTab;
            }
            else
            {
                SelectedTab = existingTab;
            }
        }

        private void CloseCurrent()
        {
            if (_projectHelper.CloseCurrentProject(IsProjectDirty ? null : false))
            {
                // instead of duplicating cleanup logic – reuse the new helper
                CreateNewProjectUi();
            }
        }

        private void CreateNewProjectUi()
        {
            // delegate real work to the helper
            _projectHelper.CreateNewProject();

            // clear UI state
            Tabs.Clear();
            GeneratedFilesViewModels.Clear();
            MainGridLines.Clear();
            SelectedTab = null;
            SelectedMainGridItemViewModel = null;

            _currentPath = null;            // no file on disk yet

            // rebuild view-models that depend on CurrentProject
            SettingsViewModel = new SettingsViewModel(
                _projectHelper.CurrentProject.Settings,
                _projectHelper,
                new DirtyStateManager());

            _dirtyStateMgr.AddSubscription(SettingsViewModel);
            _generatedFilesHelper.Initialize(CurrentProject);

            RefreshTextPartViewModels();
            LoadGeneratedFiles();
            UpdateWindowTitle();
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

        // IDirtyTrackable implementation
        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;

        public void Dispose() => _cleanup.Dispose();
    }
}
