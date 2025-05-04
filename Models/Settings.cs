using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocCreator01.Data.Enums;
using Newtonsoft.Json;
using ReactiveUI;

namespace DocCreator01.Models
{
    public class Settings : ReactiveObject
    {
        private GenerateFileTypeEnum _genDocType = GenerateFileTypeEnum.DOCX;

        /// <summary>
        /// Type of document to generate
        /// </summary>
        [JsonProperty("genDocType")]
        public GenerateFileTypeEnum GenDocType
        {
            get => _genDocType;
            set => this.RaiseAndSetIfChanged(ref _genDocType, value);
        }
    }
}
