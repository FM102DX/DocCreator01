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
            
            // Check if script exists
            if (!File.Exists(scriptPath))
                await CreateDefaultScriptAsync(scriptPath, type);
            
            // Create temporary JSON file with TextPart data
            string tempJsonPath = Path.Combine(Path.GetTempPath(), $"docparts_{Guid.NewGuid()}.json");
            var jsonData = JsonConvert.SerializeObject(textParts.ToList(), Formatting.Indented);
            await File.WriteAllTextAsync(tempJsonPath, jsonData);
            
            // Ensure output filename has correct extension
            if (!outputFileName.EndsWith($".{type.ToString().ToLower()}", StringComparison.OrdinalIgnoreCase))
                outputFileName = $"{outputFileName}.{type.ToString().ToLower()}";
                
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
        /// Creates a default script if one doesn't exist
        /// </summary>
        private async Task CreateDefaultScriptAsync(string scriptPath, GenerateFileTypeEnum type)
        {
            // Create a basic Python script for generating documents
            StringBuilder scriptContent = new StringBuilder();
            
            switch (type)
            {
                case GenerateFileTypeEnum.DOCX:
                    scriptContent.AppendLine("#!/usr/bin/env python");
                    scriptContent.AppendLine("# -*- coding: utf-8 -*-");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("import sys");
                    scriptContent.AppendLine("import json");
                    scriptContent.AppendLine("import os");
                    scriptContent.AppendLine("from docx import Document");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("def create_docx(json_path, output_dir, output_filename):");
                    scriptContent.AppendLine("    # Load the text parts from JSON");
                    scriptContent.AppendLine("    with open(json_path, 'r', encoding='utf-8') as f:");
                    scriptContent.AppendLine("        text_parts = json.load(f)");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("    # Create a new document");
                    scriptContent.AppendLine("    doc = Document()");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("    # Add each text part to the document");
                    scriptContent.AppendLine("    for part in text_parts:");
                    scriptContent.AppendLine("        if not part.get('IncludeInDocument', True):");
                    scriptContent.AppendLine("            continue");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("        level = part.get('Level', 1)");
                    scriptContent.AppendLine("        name = part.get('Name', '')");
                    scriptContent.AppendLine("        text = part.get('Text', '')");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("        # Add heading based on level");
                    scriptContent.AppendLine("        if name:");
                    scriptContent.AppendLine("            heading_level = min(level, 9)  # docx supports headings 1-9");
                    scriptContent.AppendLine("            doc.add_heading(name, level=heading_level)");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("        # Add text content");
                    scriptContent.AppendLine("        if text:");
                    scriptContent.AppendLine("            doc.add_paragraph(text)");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("    # Save the document");
                    scriptContent.AppendLine("    output_path = os.path.join(output_dir, output_filename)");
                    scriptContent.AppendLine("    doc.save(output_path)");
                    scriptContent.AppendLine("    return output_path");
                    scriptContent.AppendLine("");
                    scriptContent.AppendLine("if __name__ == '__main__':");
                    scriptContent.AppendLine("    if len(sys.argv) != 4:");
                    scriptContent.AppendLine("        print('Usage: python create_docx.py <json_file_path> <output_directory> <output_filename>')");
                    scriptContent.AppendLine("        sys.exit(1)");
                    scriptContent.AppendLine("    ");
                    scriptContent.AppendLine("    json_path = sys.argv[1]");
                    scriptContent.AppendLine("    output_dir = sys.argv[2]");
                    scriptContent.AppendLine("    output_filename = sys.argv[3]");
                    scriptContent.AppendLine("    ");
                    scriptContent.AppendLine("    try:");
                    scriptContent.AppendLine("        output_path = create_docx(json_path, output_dir, output_filename)");
                    scriptContent.AppendLine("        print(f'Document created successfully: {output_path}')");
                    scriptContent.AppendLine("    except Exception as e:");
                    scriptContent.AppendLine("        print(f'Error creating document: {str(e)}', file=sys.stderr)");
                    scriptContent.AppendLine("        sys.exit(1)");
                    break;
                    
                default:
                    throw new NotImplementedException($"Script template for {type} is not defined.");
            }
            
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(scriptPath));
            
            // Write the script
            await File.WriteAllTextAsync(scriptPath, scriptContent.ToString());
        }
    }
}
