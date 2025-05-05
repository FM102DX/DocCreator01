using System.IO;
using System.Text;
using Newtonsoft.Json;
using DocCreator01.Contracts;
using DocCreator01.Models;

namespace DocCreator01.Data
{
    public class JsonProjectRepository : IProjectRepository
    {
        // Existing code...

        public Project Load(string path)
        {
            var jsonString = File.ReadAllText(path);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var project = JsonConvert.DeserializeObject<Project>(jsonString, settings) ?? new Project();

            // Set the file path on the project
            project.FilePath = path;

            // Filter out any generated files that no longer exist
            if (project.ProjectData.GeneratedFiles != null)
            {
                for (int i = project.ProjectData.GeneratedFiles.Count - 1; i >= 0; i--)
                {
                    if (!project.ProjectData.GeneratedFiles[i].Exists)
                    {
                        project.ProjectData.GeneratedFiles.RemoveAt(i);
                    }
                }
            }

            return project;
        }

        public void Save(Project project, string path)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            string jsonString = JsonConvert.SerializeObject(project, settings);
            File.WriteAllText(path, jsonString);
        }
    }
}