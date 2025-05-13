using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocCreator01.Models
{
    public class Project
    {
        public string Name { get; set; } = "New project";
        public Settings Settings { get; set; } = new Settings();
        public ProjectData ProjectData { get; set; } = new ProjectData();
        public List<Guid> OpenedTabs { get; set; } = new List<Guid>();
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets the folder path where the project is located (extracted from FilePath)
        /// </summary>
        [JsonIgnore]
        public string ProjectFolder => string.IsNullOrEmpty(FilePath) ? string.Empty : Path.GetDirectoryName(FilePath);

        public Project()
        {
        }
    }
}
