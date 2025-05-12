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

    public class Settings
    {

        public GenerateFileTypeEnum GenDocType { get; set; } = GenerateFileTypeEnum.HTML;


        public string DocTitle { get; set; }=String.Empty;


        public string DocDescription { get; set; } = String.Empty;


        public string DocCretaedBy { get; set; } = String.Empty;

    }

}
