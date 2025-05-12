using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DocCreator01.Contracts;

namespace DocCreator01.Models
{
    public sealed class TextPart : INumerableTextPart
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

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public bool IncludeInDocument { get; set; }

        [JsonProperty] public int Level { get; set; }

        #region jsonignore

        [JsonIgnore]
        public string Html { get; set; }

        [JsonIgnore]
        public int Order { get; set; }

        [JsonIgnore]
        public string ParagraphNo { get; set; }

        #endregion
    }
}
