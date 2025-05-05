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
        private readonly IDocGenerator _docGen;
        private readonly ITextPartHelper _textPartHelper;
        private readonly IAppPathsHelper _appPathsHelper;
        private readonly IGeneratedFilesHelper _generatedFilesHelper;
        private string? _currentPath;
        private bool _isProjectDirty;
        private Project _currentProject = new();
        private TabPageViewModel? _selectedTab;

        private TextPart? _selectedMainGridItem;
        private MainGridItemViewModel? _selectedMainGridItemViewModel;

        // Collection of TextPartListViewModels for display in the DataGrid
        public ObservableCollection<MainGridItemViewModel> MainGridLines { get; } = new();

        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTabCommand { get; }
        public ReactiveCommand<TabPageViewModel, Unit> CloseTabCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseAllTabsCommand { get; }
        public ReactiveCommand<TabPageViewModel, Unit> DeleteTabCommand { get; }
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

        public MainWindowViewModel(
            IProjectRepository repo, 
            IDocGenerator docGen, 
            ITextPartHelper textPartHelper, 
            IAppPathsHelper appPathsHelper,
            IGeneratedFilesHelper generatedFilesHelper)
        {
            _repo = repo;
            _docGen = docGen;
            _textPartHelper = textPartHelper;
            _appPathsHelper = appPathsHelper;
            _generatedFilesHelper = generatedFilesHelper;

            // Initialize SettingsViewModel
            SettingsViewModel = new SettingsViewModel(CurrentProject.Settings);

            // Subscribe to SettingsViewModel IsDirty property
            this.WhenAnyValue(x => x.SettingsViewModel.IsDirty)
                .Where(isDirty => isDirty)
                .Subscribe(_ => IsProjectDirty = true);

            OpenCommand = ReactiveCommand.Create(OpenFile, outputScheduler: Ui);
            SaveCommand = ReactiveCommand.Create(Save, outputScheduler: Ui);
            ExitCommand = ReactiveCommand.Create(() => Application.Current.Shutdown(), outputScheduler: Ui);
            AddTabCommand = ReactiveCommand.Create(AddTab, outputScheduler: RxApp.MainThreadScheduler);
            CloseTabCommand = ReactiveCommand.Create<TabPageViewModel>(vm => CloseTab(vm), outputScheduler: Ui);
            CloseAllTabsCommand = ReactiveCommand.Create(CloseAllTabs, outputScheduler: Ui);
            DeleteTabCommand = ReactiveCommand.Create<TabPageViewModel>(DeleteTab, outputScheduler: Ui);
            GenerateCommand = ReactiveCommand.Create(GenerateProject, outputScheduler: Ui);
            AddTextPartCommand = AddTabCommand;
            RemoveTextPartCommand = ReactiveCommand.Create(RemoveCurrent, outputScheduler: Ui);
            MoveUpCommand = ReactiveCommand.Create(MoveCurrentUp, outputScheduler: Ui);
            MoveDownCommand = ReactiveCommand.Create(MoveCurrentDown, outputScheduler: Ui);
            ActivateTabCommand = ReactiveCommand.Create(ActivateTab, outputScheduler: Ui);
            CloseCurrentCommand = ReactiveCommand.Create(CloseCurrent, outputScheduler: Ui);
            OpenRecentCommand = ReactiveCommand.Create<string>(OpenRecent, outputScheduler: Ui);
            MoveLeftCommand = ReactiveCommand.Create(MoveCurrentLeft, outputScheduler: Ui);
            MoveRightCommand = ReactiveCommand.Create(MoveCurrentRight, outputScheduler: Ui);

            // Subscribe to changes in the TextParts collection
            this.WhenAnyValue(x => x.CurrentProject)
                .Subscribe(_ => _textPartHelper.RefreshTextPartViewModels(CurrentProject.ProjectData.TextParts, MainGridLines));

            LoadRecentFiles(); // Load recent files on startup
        }

        public bool IsProjectDirty
        {
            get => _isProjectDirty;
            private set
            {
                this.RaiseAndSetIfChanged(ref _isProjectDirty, value);
                UpdateWindowTitle();                 // сразу обновляем заголовок
            }
        }
        public string AppDataDir => _appPathsHelper.AppDataDirectory;

        public Project CurrentProject
        {
            get => _currentProject;
            private set => this.RaiseAndSetIfChanged(ref _currentProject, value);
        }

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
        public ObservableCollection<TabPageViewModel> Tabs { get; } = new();
        public ObservableCollection<GeneratedFileViewModel> GeneratedFileViewModels { get; } = new();
        public TabPageViewModel? SelectedTab
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

        private void SubscribeTab(TabPageViewModel vm)
        {
            // любое IsDirty=true на вкладке делает «грязным» весь проект
            vm.WhenAnyValue(x => x.IsDirty)
                .Where(d => d)                     // интересует только переход в true
                .Subscribe(_ => IsProjectDirty = true);
        }

        private IScheduler Ui => RxApp.MainThreadScheduler;

        private void RefreshTextPartViewModels()
        {
            _textPartHelper.RefreshTextPartViewModels(CurrentProject.ProjectData.TextParts, MainGridLines);
        }

        private void AddTab()
        {
            var tp = _textPartHelper.CreateTextPart(CurrentProject);
            CurrentProject.ProjectData.TextParts.Add(tp);

            // Add corresponding view model
            var listViewModel = new MainGridItemViewModel(tp);
            MainGridLines.Add(listViewModel);

            var vm = new TabPageViewModel(tp);
            SubscribeTab(vm);
            Tabs.Add(vm);
            SelectedTab = vm;

            IsProjectDirty = true;          // structure changed
        }

        private void CloseTab(TabPageViewModel? vm) => Tabs.Remove(vm!);
        private void CloseAllTabs() => Tabs.Clear();

        private void DeleteTab(TabPageViewModel? vm)
        {
            if (vm == null) return;

            _textPartHelper.RemoveTextPart(vm.TextPart, CurrentProject.ProjectData.TextParts, MainGridLines);
            CloseTab(vm);
            IsProjectDirty = true;
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
            CurrentProject.ProjectData.TextParts.Clear();
            MainGridLines.Clear();
            Tabs.Clear();

            CurrentProject = _textPartHelper.LoadProject(fileName);
            _currentPath = fileName;

            // Update SettingsViewModel with new project's settings
            SettingsViewModel = new SettingsViewModel(CurrentProject.Settings);
            this.RaisePropertyChanged(nameof(SettingsViewModel));

            // Create view models for each text part
            RefreshTextPartViewModels();

            // Load generated files
            LoadGeneratedFiles();

            AddRecent(fileName);
            UpdateWindowTitle();
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
            CurrentProject.OpenedTabs = Tabs.Select(x => x.TextPart.Id).ToList();
            _textPartHelper.SaveProject(CurrentProject, _currentPath!);
            AddRecent(_currentPath);
            foreach (var tab in Tabs)
                tab.AcceptChanges();

            // After saving the project, make sure to reset dirty state on SettingsViewModel
            SettingsViewModel.AcceptChanges();

            IsProjectDirty = false;
        }

        private async void GenerateProject()
        {
            try
            {
                // Filter out parts that should not be included
                var filteredProject = new Project
                {
                    Name = CurrentProject.Name,
                    Settings = CurrentProject.Settings,
                    FilePath = CurrentProject.FilePath
                };

                foreach (var part in CurrentProject.ProjectData.TextParts.Where(p => p.IncludeInDocument))
                {
                    filteredProject.ProjectData.TextParts.Add(part);
                }

                // Use the selected document type from settings
                var docType = CurrentProject.Settings.GenDocType;
                GeneratedFile outputFile = _docGen.Generate(filteredProject, docType).Result;
                
                if (!string.IsNullOrEmpty(outputFile.FileName) && File.Exists(outputFile.FileName))
                {
                    // Add to view models collection for display
                    GeneratedFileViewModels.Add(new GeneratedFileViewModel(outputFile, _appPathsHelper));
                    
                    // Mark project as dirty since we've added a generated file
                    IsProjectDirty = true;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions during document generation
                MessageBox.Show($"Error generating document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveCurrent()
        {
            if (SelectedMainGridItemViewModel == null) return;

            var textPart = SelectedMainGridItemViewModel.Model;
            var tab = Tabs.FirstOrDefault(t => t.TextPart == textPart);

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
                IsProjectDirty = true;

                // Find and mark the affected tab as dirty
                var tab = Tabs.FirstOrDefault(t => t.TextPart == textPart);
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
                IsProjectDirty = true;

                // Find and mark the affected tab as dirty
                var tab = Tabs.FirstOrDefault(t => t.TextPart == textPart);
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
            var vm = Tabs.FirstOrDefault(t => t.TextPart == tp);
            if (vm == null)
            {
                vm = new TabPageViewModel(tp);
                Tabs.Add(vm);
            }
            SelectedTab = vm;
        }

        private void CloseCurrent()
        {
            // Only ask about saving if the project has unsaved changes
            if (IsProjectDirty)
            {
                // спрашиваем пользователя, что делать с текущим документом
                var res = MessageBox.Show(
                    "Сохранить изменения перед закрытием?",
                    "Закрыть документ",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel)
                    return;               //- пользователь передумал закрывать

                if (res == MessageBoxResult.Yes)
                    Save();               //- сохраняем, затем продолжаем закрытие
            }

            // ------------------ «обычное» закрытие ------------------
            _currentPath = null;
            UpdateWindowTitle();
            Tabs.Clear();
            GeneratedFileViewModels.Clear();
            CurrentProject.ProjectData.TextParts.Clear();
            MainGridLines.Clear();
            SelectedTab = null;
            SelectedMainGridItemViewModel = null;
            IsProjectDirty = false;
            CurrentProject = new Project();  // создаём новый пустой проект с именем "New project" и пустым FilePath
            SettingsViewModel = new SettingsViewModel(CurrentProject.Settings);
            this.RaisePropertyChanged(nameof(SettingsViewModel));
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
            _generatedFilesHelper.RefreshGeneratedFileViewModels(CurrentProject.ProjectData.GeneratedFiles, GeneratedFileViewModels);
        }

        // Add this property to MainWindowViewModel class
        public SettingsViewModel SettingsViewModel { get; private set; }
    }
}
