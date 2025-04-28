using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.ViewModels
{
    public sealed class SettingsViewModel : ReactiveObject
    {
        public Settings Model { get; }

        public SettingsViewModel(Settings model) => Model = model;

        // пример обёртки, добавьте реальные свойства по мере появления
        // public bool SomeFlag
        // {
        //     get => Model.SomeFlag;
        //     set => this.RaiseAndSetIfChanged(ref Model.SomeFlag, value);
        // }
    }
}
