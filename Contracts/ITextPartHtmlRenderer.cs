using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    public interface ITextPartHtmlRenderer
    {
        void RenderHtml(IEnumerable<TextPart> parts);
    }
}
