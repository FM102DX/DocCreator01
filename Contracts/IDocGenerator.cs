using DocCreator01.Data.Enums;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    public interface IDocGenerator
    {
        /// <summary>
        /// Generates a document based on the project and returns the path to the generated file
        /// </summary>
        /// <param name="project">The project to generate</param>
        /// <param name="type">The type of document to generate</param>
        /// <returns>The path to the generated file</returns>
        Task<GeneratedFile> Generate(Project project, GenerateFileTypeEnum type);
    }
}
