using DocCreator01.Models;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DocCreator01.ViewModels
{
    public sealed class MainGridItemViewModel : ReactiveObject
    {
        private readonly TextPart _textPart;
        
        public MainGridItemViewModel(TextPart textPart)
        {
            _textPart = textPart;
            
            // Subscribe to changes in the Level property to update dependent properties
            this.WhenAnyValue(x => x._textPart.Level)
                .Subscribe(_ => 
                {
                    this.RaisePropertyChanged(nameof(Indentation));
                    this.RaisePropertyChanged(nameof(Level));
                });
        }

        // Reference to the underlying model
        public TextPart Model => _textPart;
        
        // Display properties
        public string Name => _textPart.Name;
        public int Level => _textPart.Level;
        
        // String with double hyphens representing the indentation level
        public string Indentation
        {
            get
            {
                if (_textPart.Level <= 1)
                    return string.Empty;
                    
                // Create white double hyphens for indentation
                // For Level=2: "--"
                // For Level=3: "----"
                // For Level=4: "------"
                return new string('-', (_textPart.Level - 1) * 2);
            }
        }
        
    }
}
