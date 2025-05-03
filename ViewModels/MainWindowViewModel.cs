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

namespace DocCreator01.ViewModel
{
    public sealed class MainWindowViewModel : ReactiveObject
    {
        private readonly IProjectRepository _repo;
        private readonly IDocGenerator _docGen;
        private string? _currentPath;
        private bool _isProjectDirty;
        private Project _currentProject = new();
        private TabPageViewModel? _selectedTab;
        public bool IsProjectDirty
        {
            get => _isProjectDirty;
            private set
            {
                this.RaiseAndSetIfChanged(ref _isProjectDirty, value);
                UpdateWindowTitle();                 // сразу обновляем заголовок
            }
        }
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
        public ReactiveCommand<TextPart, Unit> ActivateTabCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCurrentCommand { get; }
        public MainWindowViewModel(IProjectRepository repo, IDocGenerator docGen)
        {
            _repo = repo;
            _docGen = docGen;
            OpenCommand = ReactiveCommand.Create(OpenFile, outputScheduler: Ui);
            SaveCommand = ReactiveCommand.Create(Save, outputScheduler: Ui);
            ExitCommand = ReactiveCommand.Create(() => Application.Current.Shutdown(), outputScheduler: Ui);
            AddTabCommand = ReactiveCommand.Create(AddTab, outputScheduler: RxApp.MainThreadScheduler);
            CloseTabCommand = ReactiveCommand.Create<TabPageViewModel>(vm => CloseTab(vm), outputScheduler: Ui);
            CloseAllTabsCommand = ReactiveCommand.Create(CloseAllTabs, outputScheduler: Ui);
            DeleteTabCommand = ReactiveCommand.Create<TabPageViewModel>(DeleteTab, outputScheduler: Ui);
            GenerateCommand = ReactiveCommand.Create(Generate, outputScheduler: Ui);
            AddTextPartCommand = AddTabCommand;
            RemoveTextPartCommand = ReactiveCommand.Create(RemoveCurrent, outputScheduler: Ui);
            MoveUpCommand = ReactiveCommand.Create(MoveCurrentUp, outputScheduler: Ui);
            MoveDownCommand = ReactiveCommand.Create(MoveCurrentDown, outputScheduler: Ui);
            ActivateTabCommand = ReactiveCommand.Create<TextPart>(ActivateTab, outputScheduler: Ui);
            CloseCurrentCommand = ReactiveCommand.Create(CloseCurrent, outputScheduler: Ui);
            OpenRecentCommand = ReactiveCommand.Create<string>(OpenRecent, outputScheduler: Ui);
            AppDataDir = GetProgramDataPath();

            LoadRecentFiles(); // Load recent files on startup
        }

        public string AppDataDir { get; private set; }

        public Project CurrentProject
        {
            get => _currentProject;
            private set => this.RaiseAndSetIfChanged(ref _currentProject, value);
        }
        public ObservableCollection<string> RecentFiles { get; } = new();

        public ObservableCollection<TabPageViewModel> Tabs { get; } = new();
        public ObservableCollection<string> GeneratedDocs { get; } = new();
        public TabPageViewModel? SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }
        private IScheduler Ui => RxApp.MainThreadScheduler;

        // добавление пути в список (макс. 5 элементов)
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
        public string WindowTitle =>
            string.IsNullOrEmpty(_currentPath)
                ? $"{CurrentProject.Name}{(IsProjectDirty ? " *" : "")}"
                : $"{CurrentProject.Name} - {Path.GetFileName(_currentPath)}{(IsProjectDirty ? " *" : "")}";

        // маленький помощник, чтобы не писать RaisePropertyChanged много раз
        private void UpdateWindowTitle() => this.RaisePropertyChanged(nameof(WindowTitle));
        private void SubscribeTab(TabPageViewModel vm)
        {
            // любое IsDirty=true на вкладке делает «грязным» весь проект
            vm.WhenAnyValue(x => x.IsDirty)
                .Where(d => d)                     // интересует только переход в true
                .Subscribe(_ => IsProjectDirty = true);
        }
        private void AddTab()
        {
            var tp = new TextPart
            {
                Id = Guid.NewGuid(),
                Title = CurrentProject.GetNewTextPartName(),
                Text = $"Tab {CurrentProject.ProjectData.TextParts.Count + 1}"
            };
            CurrentProject.ProjectData.TextParts.Add(tp);

            var vm = new TabPageViewModel(tp);
            SubscribeTab(vm);               // ← новый вызов
            Tabs.Add(vm);
            SelectedTab = vm;

            IsProjectDirty = true;          // структура изменилась
        }
        private void CloseTab(TabPageViewModel? vm) => Tabs.Remove(vm!);
        private void CloseAllTabs() => Tabs.Clear();
        private void DeleteTab(TabPageViewModel? vm)
        {
            if (vm == null) return;
            CurrentProject.ProjectData.TextParts.Remove(vm.TextPart);
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
            // очистка старого состояния
            CurrentProject.ProjectData.TextParts.Clear();
            Tabs.Clear();
            CurrentProject = _repo.Load(fileName);
            _currentPath = fileName;
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
                    DefaultExt = ".docparts"
                };
                if (dlg.ShowDialog() == true)
                {
                    _currentPath = dlg.FileName;
                    UpdateWindowTitle();
                }
                else return;
            }
            CurrentProject.OpenedTabs = Tabs.Select(x => x.TextPart.Id).ToList();
            _repo.Save(CurrentProject, _currentPath!);
            AddRecent(_currentPath);      // ← новая строка
            foreach (var tab in Tabs)
                tab.AcceptChanges();

            IsProjectDirty = false;
        }

        private void Generate()
        {
            _docGen.Generate(CurrentProject, GenerateFileTypeEnum.DOCX);
            GeneratedDocs.Add($"Doc{GeneratedDocs.Count + 1:D2}.docx");
        }

        private void RemoveCurrent()
        {
            if (SelectedTab == null) return;
            CurrentProject.ProjectData.TextParts.Remove(SelectedTab.TextPart);
            Tabs.Remove(SelectedTab);
            SelectedTab = Tabs.FirstOrDefault();
            IsProjectDirty = true;
        }

        private void MoveCurrentUp()
        {
            if (SelectedTab == null) return;
            var list = CurrentProject.ProjectData.TextParts;
            int idx = list.IndexOf(SelectedTab.TextPart);
            if (idx > 0)
            {
                list.Move(idx, idx - 1);
                Tabs.Move(idx, idx - 1);
            }
            IsProjectDirty = true;
        }

        private void MoveCurrentDown()
        {
            if (SelectedTab == null) return;
            var list = CurrentProject.ProjectData.TextParts;
            int idx = list.IndexOf(SelectedTab.TextPart);
            if (idx < list.Count - 1 && idx >= 0)
            {
                list.Move(idx, idx + 1);
                Tabs.Move(idx, idx + 1);
            }
            IsProjectDirty = true;
        }

        private void ActivateTab(TextPart tp)
        {
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
            GeneratedDocs.Clear();
            CurrentProject.ProjectData.TextParts.Clear();
            SelectedTab = null;
            IsProjectDirty = false;
            CurrentProject = new Project();  // создаём новый пустой проект с именем "New project"
        }

        private string GetProgramDataPath()
        {
            string company = "RICompany";
            string product = "DockPartApp";

            string docsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                company, product);

            Directory.CreateDirectory(docsDir);          // прав администратора не нужно
            return docsDir;
        }

        private void LoadRecentFiles()
        {
            string filePath = Path.Combine(AppDataDir, "appdata.docpartsettings");
            if (File.Exists(filePath))
            {
                var recentFiles = File.ReadAllLines(filePath);
                foreach (var file in recentFiles)
                {
                    if (!string.IsNullOrWhiteSpace(file))
                        RecentFiles.Add(file);
                }
            }
        }

        private void SaveRecentFiles()
        {
            string filePath = Path.Combine(AppDataDir, "appdata.docpartsettings");
            File.WriteAllLines(filePath, RecentFiles);
        }
    }
}
