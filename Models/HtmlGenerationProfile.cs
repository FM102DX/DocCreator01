using DocCreator01.Models;
using System;

namespace DocCreator01.Models
{
    public class HtmlGenerationProfile
    {
        public int HtmlFontSize { get; set; } = 14;
        public ElementSpacingInfo HtmlH1Margins { get; set; }
        public ElementSpacingInfo HtmlH2Margins { get; set; }
        public ElementSpacingInfo HtmlH3Margins { get; set; }
        public ElementSpacingInfo HtmlH4Margins { get; set; }
        public ElementSpacingInfo HtmlH5Margins { get; set; }
        public ElementSpacingInfo HtmlTableCellPaddings { get; set; }
    }
}