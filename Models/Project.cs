using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Models
{
    public class Project
    {
        public Settings Settings { get; set; } = new Settings();
        public ProjectData ProjectData { get; set; } = new ProjectData();
    }
}
