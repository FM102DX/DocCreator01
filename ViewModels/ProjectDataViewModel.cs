using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.ViewModels
{
    public sealed class ProjectDataViewModel : ReactiveObject
    {
        public ProjectData Model { get; }

        // Коллекция именно VM, чтобы UI получал уведомления от ReactiveUI
        public ObservableCollection<TextPartViewModel> TextParts { get; }

        public ProjectDataViewModel(ProjectData model)
        {
            Model = model;

            // Конвертируем каждую доменную TextPart → TextPartViewModel
            TextParts = new ObservableCollection<TextPartViewModel>(
                Model.TextParts.Select(tp => new TextPartViewModel(tp)));

            /* Синхронизация: если из UI добавят/удалят VM, меняем и доменную коллекцию */
            TextParts.CollectionChanged += (_, e) =>
            {
                if (e.NewItems != null)
                    foreach (TextPartViewModel vm in e.NewItems)
                        Model.TextParts.Add(vm.Model);

                if (e.OldItems != null)
                    foreach (TextPartViewModel vm in e.OldItems)
                        Model.TextParts.Remove(vm.Model);
            };
        }

        /* Удобные методы-обёртки */
        public TextPartViewModel AddNew(string initialText = "")
        {
            var vm = new TextPartViewModel(new TextPart { Text = initialText });
            TextParts.Add(vm);
            return vm;
        }

        public void Remove(TextPartViewModel vm) => TextParts.Remove(vm);
    }
}
