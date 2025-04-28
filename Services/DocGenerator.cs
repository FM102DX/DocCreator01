using DocCreator01.Contracts;
using DocCreator01.Data.Enums;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public sealed class DocGenerator : IDocGenerator
    {
        public void Generate(Project project, GenerateFileTypeEnum type)
        {
            // TODO: реальная генерация DOCX
        }
    }
}
