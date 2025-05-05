using DocCreator01.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    public interface IHtmlDocumentCreatorService
    {
        Task<string> CreateDocumentAsync(IEnumerable<TextPart> textParts, string outputFileName);
        string OutputDirectory { get; }
    }
}
