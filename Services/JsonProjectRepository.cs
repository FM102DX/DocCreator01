using DocCreator01.Contracts;
using DocCreator01.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public sealed class JsonProjectRepository : IProjectRepository
    {
        public Project Load(string path)
        {
            if (!File.Exists(path)) return new Project();
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Project>(json) ?? new Project();
        }

        public void Save(Project project, string path)
        {
            var json = JsonConvert.SerializeObject(project, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
