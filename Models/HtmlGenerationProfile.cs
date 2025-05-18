using DocCreator01.Data.Enums;
using DocCreator01.Models;

public class HtmlGenerationProfile : IEquatable<HtmlGenerationProfile>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int HtmlFontSize { get; set; } = 14;
    public ElementSpacingInfo HtmlH1Margins { get; set; }
    public ElementSpacingInfo HtmlH2Margins { get; set; }
    public ElementSpacingInfo HtmlH3Margins { get; set; }
    public ElementSpacingInfo HtmlH4Margins { get; set; }
    public ElementSpacingInfo HtmlH5Margins { get; set; }
    public ElementSpacingInfo HtmlTableCellPaddings { get; set; }
    public ElementSpacingInfo HtmlDocumentPaddings { get; set; }
    public string TableHeaderColor { get; set; } = "#F1F3F6";
    public HtmlGenerationPatternEnum HtmlGenerationPattern { get; set; } = HtmlGenerationPatternEnum.PlainBlueHeader;
    public override bool Equals(object obj) => Equals(obj as HtmlGenerationProfile);

    public bool Equals(HtmlGenerationProfile other)
        => other != null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}