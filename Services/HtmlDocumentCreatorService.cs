using DocCreator01.Contracts;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public class HtmlDocumentCreatorService : IHtmlDocumentCreatorService
    {
        private readonly IAppPathsHelper _appPathsHelper;

        public HtmlDocumentCreatorService(IAppPathsHelper appPathsHelper)
        {
            _appPathsHelper = appPathsHelper ?? throw new ArgumentNullException(nameof(appPathsHelper));
        }

        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        public string OutputDirectory => _appPathsHelper.DocumentsOutputDirectory;

        /// <summary>
        /// Creates an HTML document by combining HTML content from TextParts
        /// </summary>
        public async Task<string> CreateDocumentAsync(IEnumerable<TextPart> textParts, string outputFileName)
        {
            try
            {
                // Ensure the output directory exists
                Directory.CreateDirectory(OutputDirectory);
                
                // Full path to output file
                string outputFilePath = Path.Combine(OutputDirectory, outputFileName);
                
                // Create HTML document with header and combine all HTML parts
                var htmlBuilder = new StringBuilder();
                htmlBuilder.AppendLine("<!DOCTYPE html>");
                htmlBuilder.AppendLine("<html>");
                htmlBuilder.AppendLine("<head>");
                htmlBuilder.AppendLine("<meta charset=\"UTF-8\">");
                htmlBuilder.AppendLine("<title>Generated Document</title>");
                htmlBuilder.AppendLine("<style>");
                htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }");
                htmlBuilder.AppendLine("h1 { color: #333366; }");
                htmlBuilder.AppendLine("h2 { color: #333366; }");
                htmlBuilder.AppendLine("h3 { color: #333366; }");
                htmlBuilder.AppendLine("</style>");
                htmlBuilder.AppendLine("</head>");
                htmlBuilder.AppendLine("<body>");
                
                // Add each TextPart's HTML content
                foreach (var part in textParts)
                {
                    // Use either the HTML content if available, or convert the Text to basic HTML
                    string htmlContent = !string.IsNullOrEmpty(part.Html) ? 
                        part.Html : 
                        $"<h{part.Level}>{part.Name}</h{part.Level}><div>{part.Text.Replace(Environment.NewLine, "<br>")}</div>";
                    
                    htmlBuilder.AppendLine(htmlContent);
                    htmlBuilder.AppendLine("<hr>");
                }
                
                htmlBuilder.AppendLine("</body>");
                htmlBuilder.AppendLine("</html>");
                
                // Write to file
                string x = htmlBuilder.ToString();
                await File.WriteAllTextAsync(outputFilePath, x);
                
                return outputFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating HTML document: {ex.Message}", ex);
            }
        }
    }
}
