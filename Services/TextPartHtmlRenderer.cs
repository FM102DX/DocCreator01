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

            // Work only with TextPartChunks
            if (part.TextPartChunks != null && part.TextPartChunks.Count > 0)
            {
                var chunkHtmls = new List<string>();
                
                foreach (var chunk in part.TextPartChunks)
                {
                    string chunkHtml = BuildChunkHtml(chunk);
                    if (!string.IsNullOrWhiteSpace(chunkHtml))
                    {
                        chunkHtmls.Add(chunkHtml);
                    }
                }
                
                string combinedBody = string.Join("\n", chunkHtmls);
                string partHeading = BuildHeading(part.Name, part.Level, part.ParagraphNo);
                return $"{partHeading}\n{combinedBody}".Trim();
            }
            
            // If no chunks, return only heading
            string heading = BuildHeading(part.Name, part.Level, part.ParagraphNo);
            return heading;
        }

        /// <summary>
        /// Builds HTML for a single TextPartChunk, handling both text and image content
        /// </summary>
        private string BuildChunkHtml(TextPartChunk chunk)
        {
            if (chunk == null) return string.Empty;

            // Handle image chunks
            if (chunk.HasImage && chunk.ImageData != null)
            {
                return ConvertImageToHtml(chunk.ImageData);
            }

            // Handle text chunks
            if (!string.IsNullOrWhiteSpace(chunk.Text))
            {
                var detected = DetectFormat(chunk.Text);

                return detected switch
                {
                    TextFormat.Html      => ExtractBodyContent(chunk.Text),
                    TextFormat.Markdown  => Markdown.ToHtml(chunk.Text, _pipeline),
                    _                    => ConvertPlainText(chunk.Text)
                };
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts image byte array to inline HTML img tag with base64 encoding
        /// </summary>
        private static string ConvertImageToHtml(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return string.Empty;

            try
            {
                // Determine image format based on first few bytes
                string mimeType = GetImageMimeType(imageData);
                string base64String = Convert.ToBase64String(imageData);
                
                return $"<img src=\"data:{mimeType};base64,{base64String}\" style=\"max-width: 100%; height: auto;\" />";
            }
            catch (Exception)
            {
                // If conversion fails, return empty string or error message
                return "<p><em>Error: Unable to display image</em></p>";
            }
        }

        /// <summary>
        /// Determines MIME type based on image file signature
        /// </summary>
        private static string GetImageMimeType(byte[] imageData)
        {
            if (imageData.Length < 4) return "image/png"; // default

            // Check for common image signatures
            if (imageData[0] == 0xFF && imageData[1] == 0xD8) return "image/jpeg";
            if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47) return "image/png";
            if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46) return "image/gif";
            if (imageData[0] == 0x42 && imageData[1] == 0x4D) return "image/bmp";
            if (imageData.Length >= 12 && 
                imageData[0] == 0x52 && imageData[1] == 0x49 && imageData[2] == 0x46 && imageData[3] == 0x46 &&
                imageData[8] == 0x57 && imageData[9] == 0x45 && imageData[10] == 0x42 && imageData[11] == 0x50) return "image/webp";

            return "image/png"; // default fallback
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

        private static string BuildHeading(string name, int level, string? paragraphNo = null)
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
