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
        private readonly IHtmlDocumentCreatorService _htmlDocumentCreator;
        private readonly IBrowserService _browserService;
        private readonly ITextPartHtmlRenderer _textPartHtmlRenderer;

        public DocGenerator(
            IPythonHelper pythonHelper, 
            IHtmlDocumentCreatorService htmlDocumentCreator,
            IBrowserService browserService,
            ITextPartHtmlRenderer textPartHtmlRenderer)
        {
            _pythonHelper = pythonHelper ?? throw new ArgumentNullException(nameof(pythonHelper));
            _htmlDocumentCreator = htmlDocumentCreator ?? throw new ArgumentNullException(nameof(htmlDocumentCreator));
            _browserService = browserService ?? throw new ArgumentNullException(nameof(browserService));
            _textPartHtmlRenderer = textPartHtmlRenderer ?? throw new ArgumentNullException(nameof(textPartHtmlRenderer));
        }

        public async void Generate(Project project, GenerateFileTypeEnum type)
        {
            try
            {
                // Only include text parts marked for inclusion
                var parts = project.ProjectData.TextParts.Where(p => p.IncludeInDocument && !string.IsNullOrEmpty(p.Text)).ToList();
                
                
                if (!parts.Any())
                {
                    // Handle case when no parts are selected for inclusion
                    throw new InvalidOperationException("No text parts are selected for inclusion in the document.");
                }

                // Render HTML for all text parts
                _textPartHtmlRenderer.RenderHtml(parts);

                // Generate a filename based on project name
                string fileName = $"{project.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.{type.ToString().ToLower()}";
                string filePath = string.Empty;
                
                // Branch based on document type
                switch (type)
                {
                    case GenerateFileTypeEnum.DOCX:
                        // Create the document using Python helper for DOCX
                        filePath = await _pythonHelper.CreateDocumentAsync(type, parts, fileName);
                        break;
                        
                    case GenerateFileTypeEnum.HTML:
                        // Create the document using HTML document creator service
                        filePath = await _htmlDocumentCreator.CreateDocumentAsync(parts, fileName);
                        
                        // Open HTML document in Opera browser
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            _browserService.OpenInOpera(filePath);
                        }
                        break;
                        
                    default:
                        throw new NotSupportedException($"Document type {type} is not supported.");
                }
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
