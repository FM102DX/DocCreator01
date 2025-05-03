using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocCreator01.Models
{
    public sealed class TextPart : ReactiveObject   // ← теперь ReactiveObject
    {
        string _title = "";
        string _text = "";
        string _name = "";
        bool _includeInDocument = true;
        
        public TextPart()
        {
            
        }
        [JsonConstructor]                       // для Newtonsoft.Json
        public TextPart(Guid id, string title, string text, string name = "", bool includeInDocument = true)
        {
            Id = id;
            Title = title;
            Text = text;
            Name = name;
            IncludeInDocument = includeInDocument;
        }
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        [JsonProperty]
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        [JsonProperty]
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        [JsonProperty]
        public bool IncludeInDocument
        {
            get => _includeInDocument;
            set => this.RaiseAndSetIfChanged(ref _includeInDocument, value);
        }
    }

}
