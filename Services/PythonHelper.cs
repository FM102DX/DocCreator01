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
        private readonly string _scriptsDirectory;
        
        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        public string OutputDirectory { get; }

        public PythonHelper()
        {
            // Default to system Python - in production, this might come from configuration
            _pythonExecutable = "python";
            
            // Script locations relative to the application
            _scriptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
            
            // Create scripts directory if it doesn't exist
            if (!Directory.Exists(_scriptsDirectory))
                Directory.CreateDirectory(_scriptsDirectory);
                
            // Create output directory for generated documents
            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DocCreator", "Generated");
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);
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
            string scriptPath = Path.Combine(_scriptsDirectory, scriptName);
            
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
    }
}
