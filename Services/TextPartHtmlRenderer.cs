using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocCreator01.Contracts;
using Markdig;
using DocCreator01.Models;

namespace DocCreator01.Services
{


    /// <summary>
    /// Реализация ITextPartHtmlRenderer.
    /// </summary>
    public sealed class TextPartHtmlRenderer : ITextPartHtmlRenderer
    {
        private readonly MarkdownPipeline _pipeline;

        public TextPartHtmlRenderer()
        {
            // Таблицы, расширенные ссылки и пр.
            _pipeline = new MarkdownPipelineBuilder()
                            .UseAdvancedExtensions()
                            .Build();
        }

        public void RenderHtml(IEnumerable<TextPart> parts)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));

            foreach (var part in parts)
            {
                part.Html = BuildHtml(part);
            }
        }

        /* ---------- основная «кухня» ---------- */

        private string BuildHtml(TextPart part)
        {
            if (part is null) return string.Empty;

            var detected = DetectFormat(part.Text);

            string body = detected switch
            {
                TextFormat.Html      => part.Text,                          // уже готовый HTML
                TextFormat.Markdown  => Markdown.ToHtml(part.Text, _pipeline),
                _                    => ConvertPlainText(part.Text)          // обычный текст
            };

            string heading = BuildHeading(part.Name, part.Level);
            return $"{heading}\n{body}".Trim();
        }

        private static string ConvertPlainText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var paragraphs = text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .Select(p => $"<p>{System.Net.WebUtility.HtmlEncode(p)}</p>");

            return string.Join(Environment.NewLine, paragraphs);
        }

        private static string BuildHeading(string name, int level)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            int lvl = Math.Clamp(level, 1, 5);
            return $"<h{lvl}>{System.Net.WebUtility.HtmlEncode(name)}</h{lvl}>";
        }

        /* ---------- определение формата ---------- */

        private static readonly Regex _htmlTagRegex =
            new(@"<([A-Za-z][A-Za-z0-9]*)\b[^>]*>(.*?)</\1>|<br\s*/?>|<img\s+[^>]+>",
                RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex _markdownRegex =
            new(@"(^|\n)\s{0,3}(#{1,6}\s)|" +      // заголовки # ### 
                @"\*{1,2}[^\*]+\*{1,2}|" +          // *italic* **bold**
                @"(?<!\!)\[[^\]]+\]\([^)]+\)|" +    // ссылки [text](url)
                @"^\s*\|.*\|\s*$",                  // таблицы | a | b |
                RegexOptions.Multiline | RegexOptions.Compiled);

        private static TextFormat DetectFormat(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return TextFormat.Plain;

            if (_htmlTagRegex.IsMatch(text))         return TextFormat.Html;
            if (_markdownRegex.IsMatch(text))        return TextFormat.Markdown;

            return TextFormat.Plain;
        }

        private enum TextFormat { Html, Markdown, Plain }
    }
}
