using DocCreator01.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.ViewModels
{
    public sealed class TextPartViewModel : ReactiveObject
    {
        public TextPart Model { get; }

        public TextPartViewModel(TextPart model) 
        {
            Model = model;
            
            // Инициализируем реактивные свойства значениями из модели
            Name = Model.Name ?? string.Empty;
            Text = Model.Text ?? string.Empty;
            Level = Model.Level;
            IncludeInDocument = Model.IncludeInDocument;
            Initialize();
        }

        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public string Text { get; set; }

        [Reactive]
        public int Level { get; set; }

        [Reactive]
        public bool IncludeInDocument { get; set; }

        // Property to display the title with proper indentation based on level
        public string DisplayTitle => new string(' ', (Level - 1) * 3) + Name;

        public void Initialize()
        {
            // Подписываемся на изменения реактивных свойств и синхронизируем с моделью
            this.WhenAnyValue(x => x.Name)
                .Subscribe(value => Model.Name = value);

            this.WhenAnyValue(x => x.Text)
                .Subscribe(value => Model.Text = value);

            this.WhenAnyValue(x => x.Level)
                .Subscribe(value => Model.Level = value);

            this.WhenAnyValue(x => x.IncludeInDocument)
                .Subscribe(value => Model.IncludeInDocument = value);

            // Обновляем DisplayTitle при изменении Name или Level
            this.WhenAnyValue(x => x.Name, x => x.Level)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(DisplayTitle)));
        }
    }
}
