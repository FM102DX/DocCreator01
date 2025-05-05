using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using System;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public class DocGenerator : IDocGenerator
    {
        private readonly IGeneratedFilesHelper _generatedFilesHelper;

        public DocGenerator(IGeneratedFilesHelper generatedFilesHelper)
        {
            _generatedFilesHelper = generatedFilesHelper ?? throw new ArgumentNullException(nameof(generatedFilesHelper));
        }

        public async Task<GeneratedFile> Generate(Project project, GenerateFileTypeEnum type)
        {
            try
            {
                // Use the GeneratedFilesHelper to generate the file
                string filePath = await _generatedFilesHelper.GenerateFileAsync(project, type);

                // Create and add generated file record to project if file was successfully created
                if (!string.IsNullOrEmpty(filePath))
                {
                    var generatedFile = new GeneratedFile
                    {
                        FilePath = filePath,
                        FileType = type
                    };
                    project.ProjectData.GeneratedFiles.Add(generatedFile);
                    return generatedFile;
                }
                return new GeneratedFile();
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
