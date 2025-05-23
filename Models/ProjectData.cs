using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Models
{
    public class ProjectData
    {
        public List<TextPart> TextParts { get; set; } = new List<TextPart>();
        
        /// <summary>
        /// Collection of files that were generated for this project
        /// </summary>
        public List<GeneratedFile> GeneratedFiles { get; set; } = new List<GeneratedFile>();
    }
}
