using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public class DocGenerator : IDocGenerator
    {
        private readonly IPythonHelper _pythonHelper;

        public DocGenerator(IPythonHelper pythonHelper)
        {
            _pythonHelper = pythonHelper ?? throw new ArgumentNullException(nameof(pythonHelper));
        }

        public async void Generate(Project project, GenerateFileTypeEnum type)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project);
                
            try
            {
                // Only include text parts marked for inclusion
                var parts = project.ProjectData.TextParts.Where(p => p.IncludeInDocument).ToList();
                
                if (!parts.Any())
                {
                    // Handle case when no parts are selected for inclusion
                    throw new InvalidOperationException("No text parts are selected for inclusion in the document.");
                }

                // Generate a filename based on project name
                string fileName = $"{project.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.{type.ToString().ToLower()}";
                
                // Create the document using Python helper
                await _pythonHelper.CreateDocumentAsync(type, parts, fileName);
            }
            catch (Exception ex)
            {
                // In a real application, log the error and notify the user
                System.Diagnostics.Debug.WriteLine($"Error during document generation: {ex.Message}");
                throw;
            }
        }
    }
}
