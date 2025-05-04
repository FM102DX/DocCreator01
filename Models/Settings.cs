using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocCreator01.Data.Enums;
using Newtonsoft.Json;

namespace DocCreator01.Models
{
    public class Settings
    {
        /// <summary>
        /// Type of document to generate
        /// </summary>
        [JsonProperty("genDocType")]
        public GenerateFileTypeEnum GenDocType { get; set; } = GenerateFileTypeEnum.DOCX;
        

    }
}
