using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    public interface INumerableTextPart
    {
        int Level { get; set; }
        int Order { get; set; }
        string ParagraphNo { get; set; }
    }
}
