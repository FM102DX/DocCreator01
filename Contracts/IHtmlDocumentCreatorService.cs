using DocCreator01.Models;
using System.Collections.Generic;

namespace DocCreator01.Contracts
{
    public interface IHtmlDocumentCreatorService
    {
        string CreateDocument(IEnumerable<TextPart> textParts, string outputFileName, HtmlGenerationProfile profile = null);
        string OutputDirectory { get; }
    }
}
