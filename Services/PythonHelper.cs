using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public class PythonHelper : IPythonHelper
    {
        private readonly string _pythonExecutable;
        private readonly IAppPathsHelper _appPathsHelper;
        
        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        public string OutputDirectory => _appPathsHelper.OutputDirectory;

        public PythonHelper(IAppPathsHelper appPathsHelper)
        {
            _appPathsHelper = appPathsHelper ?? throw new ArgumentNullException(nameof(appPathsHelper));
            
            // Default to system Python - in production, this might come from configuration
            _pythonExecutable = "python";
        }

        /// <summary>
        /// Creates a document from the collection of TextPart objects
        /// </summary>
        /// <param name="type">Type of document to create</param>
        /// <param name="textParts">Collection of TextPart objects</param>
        /// <param name="outputFileName">Name of output file</param>
        /// <returns>Path to the created document</returns>
        public async Task<string> CreateDocumentAsync(GenerateFileTypeEnum type, IEnumerable<TextPart> textParts, string outputFileName)
        {
            // Determine which script to run based on document type
            string scriptName = GetScriptNameForDocType(type);
            string scriptPath = Path.Combine(_appPathsHelper.ScriptsDirectory, scriptName);
            
            // Check if script exists
            if (!File.Exists(scriptPath))
                await CreateDefaultScriptAsync(scriptPath, type);
            
            // Create temporary JSON file with TextPart data
            string tempJsonPath = Path.Combine(Path.GetTempPath(), $"docparts_{Guid.NewGuid()}.json");
            var jsonData = JsonConvert.SerializeObject(textParts.ToList(), Formatting.Indented);
            await File.WriteAllTextAsync(tempJsonPath, jsonData);
            
            // Full path to output file
            string outputFilePath = Path.Combine(OutputDirectory, outputFileName);
            
            try
            {
                // Run Python script asynchronously
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = _pythonExecutable,
                        Arguments = $"\"{scriptPath}\" \"{tempJsonPath}\" \"{OutputDirectory}\" \"{outputFileName}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    process.Start();
                    
                    // Read output and error streams asynchronously
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();

                    // Handle errors or abnormal exit code
                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Python script error (exit code {process.ExitCode}): {error}");
                    }

                    // Clean up temp file
                    if (File.Exists(tempJsonPath))
                        File.Delete(tempJsonPath);
                }
                
                return outputFilePath;
            }
            catch (Exception ex)
            {
                // Clean up temp file in case of exception
                if (File.Exists(tempJsonPath))
                    File.Delete(tempJsonPath);
                    
                throw new Exception($"Error executing Python script: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the appropriate script name based on document type
        /// </summary>
        private string GetScriptNameForDocType(GenerateFileTypeEnum type)
        {
            return type switch
            {
                GenerateFileTypeEnum.DOCX => "create_docx.py",
                _ => throw new NotImplementedException($"Document type {type} is not supported yet.")
            };
        }

        /// <summary>
        /// Creates a default script if it does not exist
        /// </summary>
        /// <param name="scriptPath">Path to the script</param>
        /// <param name="type">Type of document</param>
        private async Task CreateDefaultScriptAsync(string scriptPath, GenerateFileTypeEnum type)
        {
            string defaultScriptContent = type switch
            {
                GenerateFileTypeEnum.DOCX => "# Default Python script for DOCX generation\nprint('DOCX generation script')",
                _ => throw new NotImplementedException($"Document type {type} is not supported yet.")
            };

            await File.WriteAllTextAsync(scriptPath, defaultScriptContent);
        }
    }
}
