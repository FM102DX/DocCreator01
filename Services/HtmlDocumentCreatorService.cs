using DocCreator01.Contracts;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocCreator01.Services
{
    /// <summary>
    /// Creates HTML documents from a collection of <see cref="TextPart"/> objects.
    /// </summary>
    public class HtmlDocumentCreatorService : IHtmlDocumentCreatorService
    {
        private readonly IAppPathsHelper _appPathsHelper;

        public HtmlDocumentCreatorService(IAppPathsHelper appPathsHelper)
        {
            _appPathsHelper = appPathsHelper ?? throw new ArgumentNullException(nameof(appPathsHelper));
        }

        /// <summary>Папка, куда складываются готовые документы.</summary>
        public string OutputDirectory => _appPathsHelper.DocumentsOutputDirectory;

        /// <inheritdoc />
        public string CreateDocument(IEnumerable<TextPart> textParts, string outputFileName)
        {
            if (textParts is null) throw new ArgumentNullException(nameof(textParts));
            if (string.IsNullOrWhiteSpace(outputFileName))
                throw new ArgumentException("Имя файла обязательно.", nameof(outputFileName));

            Directory.CreateDirectory(OutputDirectory);
            string outputFilePath = Path.Combine(OutputDirectory, outputFileName);

            // Построение HTML
            string html = BuildHtml(textParts);

            // Синхронная IO-операция
            File.WriteAllText(outputFilePath, html, Encoding.UTF8);

            return outputFilePath;
        }

        /// <summary>
        /// Формирует полный HTML-документ.
        /// </summary>
        private static string BuildHtml(IEnumerable<TextPart> parts)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<title>Generated Document</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }");
            sb.AppendLine("h1, h2, h3 { color: #333366; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            foreach (var part in parts)
            {
                string htmlContent = !string.IsNullOrEmpty(part.Html)
                    ? part.Html
                    : $"<h{part.Level}>{Escape(part.Name)}</h{part.Level}><div>{Escape(part.Text).Replace(Environment.NewLine, "<br>")}</div>";

                sb.AppendLine(htmlContent);
                sb.AppendLine("<hr>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// HTML-экранирование пользовательских строк.
        /// </summary>
        private static string Escape(string? value) =>
            System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
