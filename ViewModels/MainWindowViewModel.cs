using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows;
using Microsoft.Win32;
using DocCreator01.Contracts;
using DocCreator01.Models;
using DocCreator01.Data.Enums;
using DocCreator01.ViewModels;
using System.Reactive.Concurrency;

namespace DocCreator01.ViewModel
{
    public sealed class MainWindowViewModel : ReactiveObject
    {
        readonly IProjectRepository _repo;
        readonly IDocGenerator _docGen;
        string? _currentPath;
        public Project CurrentProject { get; private set; } = new();

        public ObservableCollection<TabPageViewModel> Tabs { get; } = new();
        public ObservableCollection<string> GeneratedDocs { get; } = new();

        TabPageViewModel? _selectedTab;
        public TabPageViewModel? SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        IScheduler Ui => RxApp.MainThreadScheduler;

        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTabCommand { get; }
        public ReactiveCommand<TabPageViewModel, Unit> CloseTabCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseAllTabsCommand { get; }
        public ReactiveCommand<TabPageViewModel, Unit> DeleteTabCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateCommand { get; }

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

            OpenCommand = ReactiveCommand.Create(Open, outputScheduler: Ui);
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
        }


        public string WindowTitle =>
            string.IsNullOrEmpty(_currentPath)
                ? "DocGen App"
                : $"DocGen App - {System.IO.Path.GetFileName(_currentPath)}";

        // маленький помощник, чтобы не писать RaisePropertyChanged много раз
        void UpdateWindowTitle() => this.RaisePropertyChanged(nameof(WindowTitle));
        void UpdateCurrentProject() => this.RaisePropertyChanged(nameof(CurrentProject));

        void AddTab()
        {
            var tp = new TextPart
            {
                Id= Guid.NewGuid(),
                Title = CurrentProject.GetNewTextPartName(),
                Text = $"Tab {CurrentProject.ProjectData.TextParts.Count + 1}"
            };
            CurrentProject.ProjectData.TextParts.Add(tp);
            var vm = new TabPageViewModel(tp);
            Tabs.Add(vm);
            SelectedTab = vm;
        }

        void CloseTab(TabPageViewModel? vm) => Tabs.Remove(vm!);
        void CloseAllTabs() => Tabs.Clear();

        void DeleteTab(TabPageViewModel? vm)
        {
            if (vm == null) return;
            CurrentProject.ProjectData.TextParts.Remove(vm.TextPart);
            CloseTab(vm);
        }

        void Open()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Doc Parts (*.docparts)|*.docparts|All (*.*)|*.*",
                DefaultExt = ".docparts"
            };

            if (dlg.ShowDialog() != true) return;
            
            CurrentProject.ProjectData.TextParts.Clear();
            
            Tabs.Clear();

            CurrentProject = _repo.Load(dlg.FileName);

            _currentPath = dlg.FileName;

            UpdateWindowTitle();
            
            foreach (var tp in CurrentProject.ProjectData.TextParts)
            {
                if(CurrentProject.OpenedTabs.Contains(tp.Id))
                    Tabs.Add(new TabPageViewModel(tp));
            }
            SelectedTab = Tabs.FirstOrDefault();
            UpdateCurrentProject();
        }

        void Save()
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
        }

        void Generate()
        {
            _docGen.Generate(CurrentProject, GenerateFileTypeEnum.DOCX);
            GeneratedDocs.Add($"Doc{GeneratedDocs.Count + 1:D2}.docx");
        }

        void RemoveCurrent()
        {
            if (SelectedTab == null) return;
            CurrentProject.ProjectData.TextParts.Remove(SelectedTab.TextPart);
            Tabs.Remove(SelectedTab);
            SelectedTab = Tabs.FirstOrDefault();
        }

        void MoveCurrentUp()
        {
            if (SelectedTab == null) return;
            var list = CurrentProject.ProjectData.TextParts;
            int idx = list.IndexOf(SelectedTab.TextPart);
            if (idx > 0)
            {
                list.Move(idx, idx - 1);
                Tabs.Move(idx, idx - 1);
            }
        }

        void MoveCurrentDown()
        {
            if (SelectedTab == null) return;
            var list = CurrentProject.ProjectData.TextParts;
            int idx = list.IndexOf(SelectedTab.TextPart);
            if (idx < list.Count - 1 && idx >= 0)
            {
                list.Move(idx, idx + 1);
                Tabs.Move(idx, idx + 1);
            }
        }

        void ActivateTab(TextPart tp)
        {
            var vm = Tabs.FirstOrDefault(t => t.TextPart == tp);
            if (vm == null)
            {
                vm = new TabPageViewModel(tp);
                Tabs.Add(vm);
            }
            SelectedTab = vm;
        }

        void CloseCurrent()
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

            // ------------------ «обычное» закрытие ------------------
            _currentPath = null;
            UpdateWindowTitle();
            Tabs.Clear();
            GeneratedDocs.Clear();
            CurrentProject.ProjectData.TextParts.Clear();
            SelectedTab = null;

            CurrentProject = new Project();  // создаём новый пустой проект
        }

    }
}
