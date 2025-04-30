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
        public ReactiveCommand<Unit, Unit> ActivateTabCommand { get; }
        
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
            ActivateTabCommand = ReactiveCommand.Create(ActivateTab, outputScheduler: Ui);
        }

        void AddTab()
        {
            var tp = new TextPart
            {
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
            var dlg = new OpenFileDialog { Filter = "Project (*.json)|*.json|All (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            CurrentProject = _repo.Load(dlg.FileName);
            _currentPath = dlg.FileName;
            Tabs.Clear();
            foreach (var tp in CurrentProject.ProjectData.TextParts)
                Tabs.Add(new TabPageViewModel(tp));
            SelectedTab = Tabs.FirstOrDefault();
        }

        void Save()
        {
            if (string.IsNullOrEmpty(_currentPath))
            {
                var dlg = new SaveFileDialog { Filter = "Project (*.json)|*.json" };
                if (dlg.ShowDialog() == true) _currentPath = dlg.FileName;
                else return;
            }
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

        void ActivateTab()
        {

        }
    }
}
