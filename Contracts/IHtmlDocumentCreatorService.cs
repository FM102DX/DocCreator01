using DocCreator01.Models;
using System.Collections.Generic;

namespace DocCreator01.Contracts
{
    public interface IHtmlDocumentCreatorService
    {
        string CreateDocument(string outputFileName);
        string OutputDirectory { get; }
        string GenerateHtml(IEnumerable<TextPart> textParts, Project project, HtmlGenerationProfile profile);
    }
}
