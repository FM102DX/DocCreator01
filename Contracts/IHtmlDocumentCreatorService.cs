using DocCreator01.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Interface for HTML document creation service
    /// </summary>
    public interface IHtmlDocumentCreatorService
    {
        /// <summary>
        /// Creates an HTML document from a collection of TextParts
        /// </summary>
        /// <param name="textParts">Collection of TextPart objects to include in the document</param>
        /// <param name="outputFileName">Name of the output document file</param>
        /// <returns>Path to the created document file</returns>
        Task<string> CreateDocumentAsync(IEnumerable<TextPart> textParts, string outputFileName);
        
        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        string OutputDirectory { get; }
    }
}
