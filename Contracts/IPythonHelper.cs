using DocCreator01.Data.Enums;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Interface for Python script execution helper
    /// </summary>
    public interface IPythonHelper
    {
        /// <summary>
        /// Creates a document from the collection of TextPart objects
        /// </summary>
        /// <param name="type">Type of document to create (e.g. DOCX)</param>
        /// <param name="textParts">Collection of TextPart objects to include in the document</param>
        /// <param name="outputFileName">Name of the output document file</param>
        /// <returns>Path to the created document file</returns>
        Task<string> CreateDocumentAsync(GenerateFileTypeEnum type, IEnumerable<TextPart> textParts, string outputFileName);
        
        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        string OutputDirectory { get; }
    }
}
