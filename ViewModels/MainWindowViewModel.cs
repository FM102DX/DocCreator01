using DocCreator01.Commands;
using DocCreator01.Contracts;
using DocCreator01.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using DocCreator01.Data.Enums;
using DocCreator01.ViewModels;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;


namespace DocCreator01.ViewModel
{
    public sealed class MainWindowViewModel : ReactiveObject
    {
        readonly IProjectRepository _repo;
        readonly IDocGenerator _docGen;

        string? _currentPath;
        Project _currentProject = new();

        public Project CurrentProject
        {
            get => _currentProject;
            private set => this.RaiseAndSetIfChanged(ref _currentProject, value);
        }

        public ObservableCollection<TabPageViewModel> Tabs { get; } = new();
        TabPageViewModel? _selectedTab;
        public TabPageViewModel? SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        public ObservableCollection<string> GeneratedDocs { get; } = new();

        // ReactiveCommands
        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> AddTabCommand { get; }
        public ReactiveCommand<TabPageViewModel, Unit> CloseTabCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseAllTabsCommand { get; }
        public ReactiveCommand<TabPageViewModel, Unit> DeleteTabCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateCommand { get; }

        public MainWindowViewModel(IProjectRepository repo, IDocGenerator docGen)
        {
            _repo = repo;
            _docGen = docGen;
            OpenCommand = ReactiveCommand.Create(Open);
            SaveCommand = ReactiveCommand.Create(Save);
            ExitCommand = ReactiveCommand.Create(() => System.Windows.Application.Current.Shutdown());
            AddTabCommand = ReactiveCommand.Create(AddTab);
            CloseTabCommand = ReactiveCommand.Create<TabPageViewModel>(CloseTab);
            CloseAllTabsCommand = ReactiveCommand.Create(CloseAllTabs);
            

            DeleteTabCommand = ReactiveCommand.Create<TabPageViewModel>(DeleteTab);
            GenerateCommand = ReactiveCommand.Create(Generate);
        }

        void AddTab()
        {
            var newTextPartName = CurrentProject.GetNewTextPartName();
            var tp = new TextPart { 
                Title= newTextPartName, 
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
            var dlg = new OpenFileDialog { Filter = "Project (*.json)|*.json|All (*.*)|*.*", Title = "Открыть проект" };
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
                var dlg = new SaveFileDialog { Filter = "Project (*.json)|*.json", Title = "Сохранить проект" };
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
    }
}
