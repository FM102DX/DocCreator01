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
using System.Runtime.InteropServices.Marshalling;
using DocCreator01.Messages;

namespace DocCreator01.ViewModels
{
    public sealed class MainWindowViewModel : ReactiveObject, IDirtyTrackable, IDisposable
    {
        private readonly IProjectRepository _repo;
        private readonly IProjectHelper _projectHelper;
        private readonly IAppPathsHelper _appPathsHelper;
        private readonly IGeneratedFilesHelper _generatedFilesHelper;
        private readonly IBrowserService _browserService;
        private readonly IDirtyStateManager _dirtyStateMgr;
        private readonly CompositeDisposable _cleanup = new();
        private Guid _selectedItemId;
        private int _selectedItemIndex;

        private string? _currentPath;
        private ITabViewModel? _selectedTab;
        private TextPart? _selectedMainGridItem;
        private MainGridItemViewModel? _selectedMainGridItemViewModel;

        public MainWindowViewModel(
            IProjectRepository repo, 
            IProjectHelper projectHelper,
            IAppPathsHelper appPathsHelper,
            IGeneratedFilesHelper generatedFilesHelper,
            IBrowserService browserService)
        {
            _repo = repo;
            _projectHelper = projectHelper;
            _appPathsHelper = appPathsHelper;
            _generatedFilesHelper = generatedFilesHelper;
            _browserService = browserService;
            _dirtyStateMgr = new DirtyStateManager();
            
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
            SaveCommand = ReactiveCommand.Create(SaveProject);
            SaveAsCommand = ReactiveCommand.Create(SaveProjectAs);
            CloseTabCommand = ReactiveCommand.Create<ITabViewModel>(CloseTab);
            DeleteTabCommand = ReactiveCommand.Create<ITabViewModel>(DeleteTab);
            ExitCommand = ReactiveCommand.Create(() => Application.Current.Shutdown());
            GenerateFileCommand = ReactiveCommand.Create(GenerateOutputFile); // Add missing initialization
            AddTextPartCommand = ReactiveCommand.Create(AddTab);
            RemoveTextPartCommand = ReactiveCommand.Create(RemoveSelectedTextPart);
            
            MoveUpCommand = ReactiveCommand.Create(MoveSelectedTextPartUp);
            MoveDownCommand = ReactiveCommand.Create(MoveSelectedTextPartDown);
            CloseCurrentCommand = ReactiveCommand.Create(CloseCurrent);
            MoveLeftCommand = ReactiveCommand.Create(MoveSelectedTextPartLeft);
            MoveRightCommand = ReactiveCommand.Create(MoveSelectedTextPartRight);
            ActivateTabCommand = ReactiveCommand.Create(ActivateSelectedItemTab);
            OpenSettingsTabCommand = ReactiveCommand.Create(OpenSettingsTab);

            OpenDocumentsFolderCommand = ReactiveCommand.Create(() => OpenFolder(_appPathsHelper.DocumentsOutputDirectory));
            OpenScriptsFolderCommand = ReactiveCommand.Create(() => OpenFolder(_appPathsHelper.ScriptsDirectory));
            OpenProjectFolderCommand = ReactiveCommand.Create(() => {
                if (!string.IsNullOrEmpty(CurrentProject.ProjectFolder))
                    OpenFolder(CurrentProject.ProjectFolder);
            });

               
            _generatedFilesHelper.Initialize(CurrentProject);
            LoadRecentFiles();

            // React when generated files are updated
            MessageBus.Current
                .Listen<GeneratedFilesUpdatedMessage>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => RefreshGeneratedFiles())
                .DisposeWith(_cleanup);

            // React when project is loaded
            MessageBus.Current
                .Listen<ProjectLoadedMessage>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(msg =>
                {
                    // Update UI when project changes
                    RefreshTextPartViewModels();
                    RefreshGeneratedFiles();

                    // Reset dirty state when project changes
                    _dirtyStateMgr.ResetDirtyState();

                    this.RaisePropertyChanged(nameof(CurrentProject));
                    this.RaisePropertyChanged(nameof(WindowTitle));
                })
                .DisposeWith(_cleanup);

            MessageBus.Current
                .Listen<AllFilesDeletedMessage>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(msg => RefreshGeneratedFiles())
                .DisposeWith(_cleanup);

        }

        #region commands

        // Collection of TextPartListViewModels for display in the DataGrid
        public ReactiveCommand<Unit, Unit> NewProjectCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTabCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<string, Unit> OpenRecentCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }
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

        #region properties

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
                {
                    SelectedMainGridItem = value.Model;
                    _selectedItemId = value.Model.Id;
                    _selectedItemIndex = MainGridLines.IndexOf(value);
                }
                else
                    SelectedMainGridItem = null;
            }
        }
        public ITextPartHelper TextPartHelper => this._projectHelper.TextPartHelper;
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

        private void SetNewSelectedItem()
        {
            //выставляет выбранный элемент после обновления грида
            if (MainGridLines.Count == 0)
            {
                SelectedMainGridItemViewModel = null;
                return;
            }

            bool elementWasSelected = _selectedItemId != Guid.Empty;
            var selectionTmp= MainGridLines.FirstOrDefault(vm => vm.Model.Id == _selectedItemId);

            bool elementWasDeleted = (selectionTmp == null && elementWasSelected);
            if (!elementWasSelected)
            {
                //если не было выделения, то первый
                SelectedMainGridItemViewModel = MainGridLines.FirstOrDefault();
            }
            if (elementWasDeleted)
            {
                //если был удаен, то по этой логике
                if (_selectedItemIndex >= 0 && _selectedItemIndex < MainGridLines.Count)
                {
                    SelectedMainGridItemViewModel = MainGridLines[_selectedItemIndex];
                }
                else if (_selectedItemIndex == MainGridLines.Count)
                {
                    //выставляем последний элемент
                    SelectedMainGridItemViewModel = MainGridLines.LastOrDefault();
                }
            }
            else
            {
                //в остальных случаях - тот же самый
                SelectedMainGridItemViewModel = selectionTmp;
            }
        }
        private void SubscribeTab(ITabViewModel vm)
        {
           if (vm is IDirtyTrackable dirtyTrackable)
           {
               _dirtyStateMgr.AddSubscription(dirtyTrackable);
           }
        }

        private IScheduler Ui => RxApp.MainThreadScheduler;
        #endregion

        #region project_manipulation

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
                    var vm = new TabPageViewModel(tp, dirtyMgr, _projectHelper);
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

        private void SaveProject()
        {
            // Get list of opened tabs
            var openedTabs = Tabs.OfType<TabPageViewModel>()
                .Select(x => x.Model.Id)
                .ToList();
            
            // Use ProjectHelper to save project
            if (_projectHelper.SaveProject(CurrentProject, openedTabs))
            {
                // Update UI state after successful save
                _currentPath = CurrentProject.FilePath;
                AddRecent(_currentPath);
                UpdateWindowTitle();
                
                // Accept all changes at once
                _dirtyStateMgr.ResetDirtyState();
                RefreshTextPartViewModels();
                SetNewSelectedItem();
            }
        }
        
        private void SaveProjectAs()
        {
            // Use ProjectHelper to handle the Save As operation
            if (_projectHelper.SaveProjectAs())
            {
                // Update UI state after successful save
                _currentPath = CurrentProject.FilePath;
                UpdateWindowTitle();
                AddRecent(_currentPath);
                
                // Accept all changes at once
                _dirtyStateMgr.ResetDirtyState();
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
        #endregion

        #region refresh_view_models

        private void RefreshTextPartViewModels()
        {
            MainGridLines.Clear();
            TextPartHelper.RefreshTextParts();
            foreach (var model in TextPartHelper.TextParts)
            {
                var vm = new MainGridItemViewModel(model);
                MainGridLines.Add(vm);
            }
        }

        private void RefreshGeneratedFilesViewModels(
            List<GeneratedFile> models,
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
        private void RefreshGeneratedFiles()
        {
            RefreshGeneratedFilesViewModels(
                CurrentProject.ProjectData.GeneratedFiles,
                GeneratedFilesViewModels);
        }
        #endregion

        #region textpart_manipulation

        private void RemoveSelectedTextPart()
        {
            if (SelectedMainGridItemViewModel == null) return;
            var textPart = SelectedMainGridItemViewModel.Model;
            var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == textPart);

            if (tab != null)
                Tabs.Remove(tab);

            TextPartHelper.RemoveTextPart(textPart);
            RefreshTextPartViewModels();
            SetNewSelectedItem();
            ActivateSelectedItemTab();
            _dirtyStateMgr.MarkAsDirty();
        }

        private void MoveSelectedTextPartUp()
        {
            if (SelectedMainGridItemViewModel == null) return;
            var textPart = SelectedMainGridItemViewModel.Model;
            if (TextPartHelper.MoveTextPartUp(textPart))
            {
                this.RaisePropertyChanged(nameof(CurrentProject));
                _dirtyStateMgr.MarkAsDirty();
                var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == textPart);
                if (tab != null && tab is IDirtyTrackable dtr)
                {
                    dtr.DirtyStateMgr.MarkAsDirty();
                }
                RefreshTextPartViewModels();
                SetNewSelectedItem();
            }
            //SelectedMainGridItem = textPart;
        }

        private void MoveSelectedTextPartDown()
        {
            if (SelectedMainGridItemViewModel == null) return;
            var textPart = SelectedMainGridItemViewModel.Model;
            if (TextPartHelper.MoveTextPartDown(textPart))
            {
                this.RaisePropertyChanged(nameof(CurrentProject));
                _dirtyStateMgr.MarkAsDirty();
                var tab = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == textPart);
                if (tab != null && tab is IDirtyTrackable dtr)
                {
                    dtr.DirtyStateMgr.MarkAsDirty();
                }
                RefreshTextPartViewModels();
                SetNewSelectedItem();
            }
            //SelectedMainGridItem = textPart;
        }

        private void MoveSelectedTextPartLeft()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (TextPartHelper.DecreaseTextPartLevel(textPart))
            {
                _dirtyStateMgr.MarkAsDirty();
                RefreshTextPartViewModels();
                SetNewSelectedItem();
            }
            //SelectedMainGridItem = textPart;
        }

        private void MoveSelectedTextPartRight()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            if (TextPartHelper.IncreaseTextPartLevel(textPart))
            {
                _dirtyStateMgr.MarkAsDirty();
                RefreshTextPartViewModels();
                SetNewSelectedItem();
            }
            //SelectedMainGridItem = textPart;
        }

        #endregion

        #region generated_files_manipulation

        private void LoadGeneratedFiles()
        {

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

        #endregion

        #region source_files_manipulation

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

            SaveRecentFiles(); // SaveProject changes to the file
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


        #endregion

        #region tabs_manipulation
        private void ActivateSelectedItemTab()
        {
            if (SelectedMainGridItem == null) return;
            var tp = SelectedMainGridItem;
            var vm = Tabs.FirstOrDefault(t => t is TabPageViewModel tpVm && tpVm.Model == tp);
            if (vm == null)
            {
                var dirtyMgr = new DirtyStateManager();
                vm = new TabPageViewModel(tp, dirtyMgr, _projectHelper);
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
        private void AddTab()
        {
            var tp = TextPartHelper.CreateTextPart();
            CurrentProject.ProjectData.TextParts.Add(tp);
            var vm = new TabPageViewModel(tp, new DirtyStateManager(), _projectHelper);
            SubscribeTab(vm);
            Tabs.Add(vm);
            SelectedTab = vm;
            _dirtyStateMgr.MarkAsDirty();
            RefreshTextPartViewModels();
            SetNewSelectedItem(); // TODO проверить правильно ли выделяется
        }

        private void CloseTab(ITabViewModel? vm) => Tabs.Remove(vm!);
        private void CloseAllTabs() => Tabs.Clear();

        private void DeleteTab(ITabViewModel? vm)
        {
            if (vm == null) return;

            // Only handle TextPart tabs
            if (vm is TabPageViewModel textPartVm)
            {
                TextPartHelper.RemoveTextPart(textPartVm.Model);
                CloseTab(vm);
                _dirtyStateMgr.MarkAsDirty();
            }
        }

        #endregion




















        public IDirtyStateManager DirtyStateMgr => _dirtyStateMgr;
        public void Dispose() => _cleanup.Dispose();
    }
}
