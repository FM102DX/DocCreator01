using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    using System.IO;
    using Newtonsoft.Json;
    using System.Text.Json;
    using System.Xml;
    using global::DocCreator01.Contracts;
    using global::DocCreator01.Models;

    namespace DocCreator01.Services
    {
        public class ProjectService : IProjectService
        {
            public Project CurrentProject { get; private set; } = new Project();

            public void Load(string path)
            {
                var json = File.ReadAllText(path);
                var proj = JsonConvert.DeserializeObject<Project>(json);
                if (proj == null)
                    throw new System.Text.Json.JsonException("Файл не содержит валидную структуру проекта.");
                CurrentProject = proj;
            }

            public void Save(string path)
            {
                var json = JsonConvert.SerializeObject(CurrentProject, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(path, json);
            }
        }
    }
}
