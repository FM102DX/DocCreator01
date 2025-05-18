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

        // Regular expression to extract content between body tags
        private static readonly Regex _bodyContentRegex =
            new(@"<body[^>]*>(.*?)</body>",
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                TextFormat.Html      => TransformHtml(part),     // извлечь только содержимое тегов body
                TextFormat.Markdown  => Markdown.ToHtml(part.Text, _pipeline),
                _                    => ConvertPlainText(part.Text)        // обычный текст
            };

            string heading = BuildHeading(part.Name, part.Level, part.ParagraphNo);
            return $"{heading}\n{body}".Trim();
        }

        private static string TransformHtml(TextPart part)
        {
            var tmp = ExtractBodyContent(part.Text);
            tmp = ShiftHtmlHeaders(tmp, part.Level);
            return tmp;
        }

        
        //Сдвигает уровни HTML-заголовков так, чтобы они были однородны и соответстовали Level
        private static string ShiftHtmlHeaders(string html, int level)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            /* 1. Определяем минимальный уровень X среди всех заголовков <hN>. */
            var headerLevels = Regex.Matches(html, @"(?i)<\s*h([1-9])\b")
                .Cast<Match>()
                .Select(m => int.Parse(m.Groups[1].Value))
                .ToList();

            if (headerLevels.Count == 0)               // заголовков нет – сдвиг не нужен
                return html;

            int minLevel = headerLevels.Min();         // X
            int delta = level - minLevel + 1;       // DELTA = Level - X + 1

            if (delta == 0)                            // уже в нужной позиции
                return html;

            /* 2. Сдвигаем уровни во всех открывающих и закрывающих тегах <hN>/<​/hN>. */
            string shifted = Regex.Replace(
                html,
                @"(?i)(</?\s*h)([1-9])(\b[^>]*>)",
                match =>
                {
                    int current = int.Parse(match.Groups[2].Value);
                    int newLevel = current + delta;

                    // Гарантируем диапазон 1–9, чтобы получить валидный тег.
                    newLevel = Math.Clamp(newLevel, 1, 9);

                    return $"{match.Groups[1].Value}{newLevel}{match.Groups[3].Value}";
                });

            return shifted;
        }



        /// <summary>
        /// Извлекает содержимое между тегами body из HTML-строки.
        /// Если теги body отсутствуют, возвращает исходную строку.
        /// </summary>
        private static string ExtractBodyContent(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var match = _bodyContentRegex.Match(html);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // Если тегов body нет, возвращаем исходную строку
            return html;
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

        private static string BuildHeading(string name, int level, string paragraphNo = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            int lvl = Math.Clamp(level, 1, 5);
            
            // Add paragraph number to the heading if available
            string headingText = !string.IsNullOrWhiteSpace(paragraphNo)
                ? $"{paragraphNo} {System.Net.WebUtility.HtmlEncode(name)}"
                : System.Net.WebUtility.HtmlEncode(name);
                
            return $"<h{lvl}>{headingText}</h{lvl}>";
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
