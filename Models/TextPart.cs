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
    public sealed class TextPart : ReactiveObject
    {
        public TextPart()
        {
            
        }
        [JsonConstructor]                       // для Newtonsoft.Json
        public TextPart(Guid id, string text, string name = "", bool includeInDocument = true, int level = 1)
        {
            Id = id;
            Text = text;
            Name = name;
            IncludeInDocument = includeInDocument;
            Level = level;
        }
        [JsonProperty]
        public Guid Id { get; set; }


        [JsonProperty]
        public string Text { get; set; }

        [JsonIgnore]
        public string Html { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public bool IncludeInDocument { get; set; }

        [JsonProperty] public int Level { get; set; }

        [JsonProperty]
        public int ParagraphNo { get; set; }

    }
}
