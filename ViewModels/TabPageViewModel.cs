using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocCreator01.Models;
using ReactiveUI;

namespace DocCreator01.ViewModels
{
    public sealed class TabPageViewModel : ReactiveObject
    {
        public TextPart TextPart { get; }

        public TabPageViewModel (TextPart textPart)
        {
            TextPart = textPart;
        }

    }
}
