using System.IO;
using System.Text;
using Newtonsoft.Json;
using DocCreator01.Contracts;
using DocCreator01.Models;

namespace DocCreator01.Data
{
    public sealed class JsonProjectRepository : IProjectRepository
    {
        private static readonly JsonSerializerSettings _json = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };

        public Project Load(string path)
        {
            if (!File.Exists(path))
                return new Project();

            var json = File.ReadAllText(path, Encoding.UTF8);
            return JsonConvert.DeserializeObject<Project>(json, _json) ?? new Project();
        }

        public void Save(Project project, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(project, _json);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
    }
}