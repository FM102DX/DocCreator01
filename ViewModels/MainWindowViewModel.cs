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


namespace DocCreator01.ViewModel
{
    public class MainWindowViewModel
    {
        private readonly IProjectService _projectService;
        public Project CurrentProject => _projectService.CurrentProject;

        public ICommand OpenCommand { get; }
        public ICommand ExitCommand { get; }

        public MainWindowViewModel(IProjectService projectService)
        {
            _projectService = projectService;
            OpenCommand = new RelayCommand(_ => Open());
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }

        private void Open()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Открыть проект"
            };

            if (dlg.ShowDialog() == true)
            {
                _projectService.Load(dlg.FileName);
                // Здесь вызвать уведомление об обновлении UI (INotifyPropertyChanged)
            }
        }
    }
}
