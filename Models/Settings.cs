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
    using Newtonsoft.Json;
    using ReactiveUI;

    public class Settings : ReactiveObject
    {
        private GenerateFileTypeEnum _genDocType = GenerateFileTypeEnum.DOCX;
        private string _docTitle;
        private string _docDescription;
        private string _docCretaedBy;

        /// <summary>
        /// Тип создаваемого документа
        /// </summary>
        [JsonProperty("genDocType")]
        public GenerateFileTypeEnum GenDocType
        {
            get => _genDocType;
            set => this.RaiseAndSetIfChanged(ref _genDocType, value);
        }

        /// <summary>
        /// Заголовок документа
        /// </summary>
        [JsonProperty("docTitle")]
        public string DocTitle
        {
            get => _docTitle;
            set => this.RaiseAndSetIfChanged(ref _docTitle, value);
        }

        /// <summary>
        /// Описание документа
        /// </summary>
        [JsonProperty("docDescription")]
        public string DocDescription
        {
            get => _docDescription;
            set => this.RaiseAndSetIfChanged(ref _docDescription, value);
        }

        /// <summary>
        /// Автор (создатель) документа
        /// </summary>
        [JsonProperty("docCretaedBy")]
        public string DocCretaedBy
        {
            get => _docCretaedBy;
            set => this.RaiseAndSetIfChanged(ref _docCretaedBy, value);
        }
    }

}
