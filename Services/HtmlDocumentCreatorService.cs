using DocCreator01.Contracts;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using DocCreator01.Data.Enums;
using HtmlAgilityPack;

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

        public string OutputDirectory => _appPathsHelper.DocumentsOutputDirectory;

        public string CreateDocument(IEnumerable<TextPart> textParts, string outputFileName, HtmlGenerationProfile profile = null)
        {
            if (textParts is null) throw new ArgumentNullException(nameof(textParts));
            if (string.IsNullOrWhiteSpace(outputFileName))
                throw new ArgumentException("Имя файла обязательно.", nameof(outputFileName));

            Directory.CreateDirectory(OutputDirectory);
            string outputFilePath = Path.Combine(OutputDirectory, outputFileName);
            string html = "";
            html = BuildHtml(textParts);
            html = ToCenteredDocument(html, profile);

            // -------------------------------------------------
            // Cell paddings come from HtmlTableCellPaddings
            // -------------------------------------------------
            var cellInfo = profile?.HtmlTableCellPaddings;
            var cellPadding = cellInfo == null
                ? (5, 5, 5, 5)
                : ((int)cellInfo.Top, (int)cellInfo.Right, (int)cellInfo.Bottom, (int)cellInfo.Left);

            switch (profile?.HtmlGenerationPattern)
            {
                default:
                case HtmlGenerationPatternEnum.AsChatGpt:
                    html = FormatTablesSlim(html, cellPadding);
                    break;
                case HtmlGenerationPatternEnum.PlainBlueHeader:
                    html = FormatTablesClassic(html, cellPadding);
                    break;
            }

            html = AjustHtmlFonts(html, profile);

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
                    : BuildDefaultHtml(part);

                sb.AppendLine(htmlContent);
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private static string BuildDefaultHtml(TextPart part)
        {
            // Add paragraph number to the heading if available
            string headingText = !string.IsNullOrWhiteSpace(part.ParagraphNo)
                ? $"{part.ParagraphNo} {Escape(part.Name)}"
                : Escape(part.Name);
                
            return $"<h{part.Level}>{headingText}</h{part.Level}><div>{Escape(part.Text).Replace(Environment.NewLine, "<br>")}</div>";
        }

        /// <summary>
        /// HTML-экранирование пользовательских строк.
        /// </summary>
        private static string Escape(string? value) =>
            System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

        /// <summary>
        /// Оборачивает контент в «лист» (.page), выравненный по центру,
        /// и задаёт его внутренние отступы согласно <see cref="HtmlGenerationProfile.HtmlDocumentPaddings"/>.
        /// Если профиль не передан – используются значения по умолчанию (40 px × 60 px).
        /// </summary>
        public static string ToCenteredDocument(string rawHtml, HtmlGenerationProfile profile = null)
        {
            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };
            doc.LoadHtml(rawHtml);

            /*-----------------------------------------------------------------
             * 1. <head> гарантированно существует
             *----------------------------------------------------------------*/
            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head == null)
            {
                head = doc.CreateElement("head");
                doc.DocumentNode.PrependChild(head);
            }

            /*-----------------------------------------------------------------
             * 2. Вычисляем значения padding для «листа»
             *----------------------------------------------------------------*/
            var pad = profile?.HtmlDocumentPaddings;   // может быть null
            double pTop = pad?.Top ?? 40;        // сверху
            double pRight = pad?.Right ?? 60;        // справа
            double pBottom = pad?.Bottom ?? 40;        // снизу
            double pLeft = pad?.Left ?? 60;        // слева

            /*-----------------------------------------------------------------
             * 3. CSS-правила
             *----------------------------------------------------------------*/
            string readerCss = $@"
html,body {{margin:0;padding:0;height:100%;background:#f2f2f2;font-family:Arial, sans-serif;line-height:1.6;}}
body      {{padding:40px 0;}}                    /* поля сверху/снизу за пределами «листа» */
.page     {{background:#fff;width:800px;         /* ≈ A4                                 */
           margin:0 auto;
           padding:{pTop}px {pRight}px {pBottom}px {pLeft}px;
           border:1px solid #ddd;
           box-shadow:0 0 10px rgba(0,0,0,.15);}}
h1,h2,h3  {{color:#333366;}}";

            // удаляем существующий инлайн-стиль, чтобы избежать дубликатов
            head.SelectSingleNode("./style[@id='center-style']")?.Remove();

            var styleNode = doc.CreateElement("style");
            styleNode.SetAttributeValue("id", "center-style");
            styleNode.InnerHtml = readerCss;
            head.AppendChild(styleNode);

            /*-----------------------------------------------------------------
             * 4. Перемещаем содержимое <body> в .page
             *----------------------------------------------------------------*/
            var body = doc.DocumentNode.SelectSingleNode("//body")
                       ?? doc.DocumentNode.AppendChild(doc.CreateElement("body"));

            var wrapper = doc.CreateElement("div");
            wrapper.SetAttributeValue("class", "page");

            foreach (var node in body.ChildNodes.ToList())
                wrapper.AppendChild(node);

            body.RemoveAllChildren();
            body.AppendChild(wrapper);

            /*-----------------------------------------------------------------
             * 5. Готово
             *----------------------------------------------------------------*/
            return doc.DocumentNode.OuterHtml;
        }


        /// <summary>
        /// Приводит все таблицы к «табличному» виду: рамки, заголовок, настраиваемые отступы.
        /// </summary>
        /// <param name="rawHtml">Исходный HTML-текст</param>
        /// <param name="padding">Отступы ячейки: (top, right, bottom, left) в пикселях</param>
        /// <returns>Преобразованный HTML-текст</returns>
        public static string FormatTablesClassic(string rawHtml, (int top, int right, int bottom, int left) padding)
        {
            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };
            doc.LoadHtml(rawHtml);

            //--------------------------------------------------
            // 1. Готовим CSS со вставкой нужных отступов
            //--------------------------------------------------
            string css = $@"
.formatted-table {{
    border-collapse: collapse;
    width: 100%;
}}
.formatted-table th,
.formatted-table td {{
    border: 1px solid #999;
    padding: {padding.top}px {padding.right}px {padding.bottom}px {padding.left}px;
}}
.formatted-table th {{
    background: #e6eff7;
    font-weight: bold;
    text-align: left;
}}";

            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head == null)
            {
                head = doc.CreateElement("head");
                doc.DocumentNode.PrependChild(head);
            }

            // добавляем <style>, если его ещё нет
            var styleNode = doc.CreateElement("style");
            styleNode.InnerHtml = css;
            head.AppendChild(styleNode);

            //--------------------------------------------------
            // 2. Обрабатываем каждую таблицу
            //--------------------------------------------------
            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    //--------------------------------------------------
                    // 2.1. Добавляем/дополняем класс formatted-table
                    //--------------------------------------------------
                    var classAttr = table.GetAttributeValue("class", "");
                    if (!classAttr.Split(' ').Contains("formatted-table"))
                    {
                        classAttr = string.IsNullOrWhiteSpace(classAttr)
                            ? "formatted-table"
                            : classAttr + " formatted-table";
                        table.SetAttributeValue("class", classAttr.Trim());
                    }

                    //--------------------------------------------------
                    // 2.2. Перемещаем первую строку в <thead>, если его нет
                    //--------------------------------------------------
                    var thead = table.SelectSingleNode("./thead");
                    if (thead == null)
                    {
                        var firstRow = table.SelectSingleNode("./tr")    // большинство «плоских» таблиц
                                    ?? table.SelectSingleNode("./tbody/tr");

                        if (firstRow != null)
                        {
                            // заменяем td на th, если нужно
                            foreach (var cell in firstRow.Elements("td").ToList())
                            {
                                var th = doc.CreateElement("th");
                                th.InnerHtml = cell.InnerHtml;
                                foreach (var attr in cell.Attributes) th.Attributes.Add(attr.Name, attr.Value);
                                cell.ParentNode.ReplaceChild(th, cell);
                            }

                            // создаём thead и помещаем строку внутрь
                            thead = doc.CreateElement("thead");
                            thead.AppendChild(firstRow.Clone());
                            firstRow.Remove();
                            table.PrependChild(thead);
                        }
                    }
                }
            }

            //--------------------------------------------------
            // 3. Отдаём готовый HTML
            //--------------------------------------------------
            return doc.DocumentNode.OuterHtml;
        }


        /// <summary>
        /// Делает «минималистичные» таблицы: только горизонтальные линии, жирный заголовок.
        /// </summary>
        /// <param name="rawHtml">Исходный HTML</param>
        /// <param name="padding">Отступы ячейки (top, right, bottom, left) в px</param>
        /// <returns>Преобразованный HTML</returns>
        public static string FormatTablesSlim(string rawHtml, (int top, int right, int bottom, int left) padding)
        {
            var doc = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };
            doc.LoadHtml(rawHtml);

            //--------------------------------------------------
            // 1. Глобальный CSS
            //--------------------------------------------------
            string css = $@"
                .slim-table {{
                    border-collapse: collapse;
                    width: 100%;
                }}
                .slim-table th,
                .slim-table td {{
                    padding: {padding.top}px {padding.right}px {padding.bottom}px {padding.left}px;
                    vertical-align: top;
                }}
                /* жирный заголовок + нижняя линия */
                .slim-table th {{
                    font-weight: 600;
                    text-align: left;
                    border-bottom: 2px solid #d0d0d0;
                }}
                /* горизонтальные разделители строк, кроме первой */
                .slim-table tbody tr + tr td {{
                    border-top: 1px solid #e5e5e5;
                }}";

            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head == null)
            {
                head = doc.CreateElement("head");
                doc.DocumentNode.PrependChild(head);
            }

            var styleNode = doc.CreateElement("style");
            styleNode.InnerHtml = css;
            head.AppendChild(styleNode);

            //--------------------------------------------------
            // 2. Обход всех таблиц
            //--------------------------------------------------
            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    // назначаем класс slim-table (не дублируем, если уже есть)
                    var classAttr = table.GetAttributeValue("class", "");
                    if (!classAttr.Split(' ').Contains("slim-table"))
                    {
                        classAttr = string.IsNullOrWhiteSpace(classAttr)
                            ? "slim-table"
                            : classAttr + " slim-table";
                        table.SetAttributeValue("class", classAttr.Trim());
                    }

                    // переносим первую строку в <thead>, заменяя <td> на <th>, если <thead> отсутствует
                    var thead = table.SelectSingleNode("./thead");
                    if (thead == null)
                    {
                        var firstRow = table.SelectSingleNode("./tr") ?? table.SelectSingleNode("./tbody/tr");
                        if (firstRow != null)
                        {
                            foreach (var cell in firstRow.Elements("td").ToList())
                            {
                                var th = doc.CreateElement("th");
                                th.InnerHtml = cell.InnerHtml;
                                foreach (var attr in cell.Attributes) th.Attributes.Add(attr.Name, attr.Value);
                                cell.ParentNode.ReplaceChild(th, cell);
                            }

                            thead = doc.CreateElement("thead");
                            thead.AppendChild(firstRow.Clone());
                            firstRow.Remove();
                            table.PrependChild(thead);
                        }
                    }
                }
            }

            //--------------------------------------------------
            // 3. Готово!
            //--------------------------------------------------
            return doc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// Корректирует шрифтовые параметры с учётом выбранного <see cref="HtmlGenerationProfile"/>.
        /// • HtmlFontSize — базовый размер шрифта всего документа (тега <body>);  
        /// • HtmlH{1–5}Margins — индивидуальные поля (margin) для заголовков h1-h5.
        /// </summary>
        /// <param name="rawHtml">Исходный HTML-текст.</param>
        /// <param name="profile">Профиль генерации HTML; если null, исходный текст возвращается без изменений.</param>
        /// <returns>Преобразованный HTML-текст.</returns>
        public static string AjustHtmlFonts(string rawHtml, HtmlGenerationProfile profile)
        {
            if (string.IsNullOrWhiteSpace(rawHtml) || profile == null)
                return rawHtml;

            // ───────────────────────────────────────────────
            // 1. Загружаем документ и гарантируем наличие <head>
            // ───────────────────────────────────────────────
            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };
            doc.LoadHtml(rawHtml);

            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head == null)
            {
                head = doc.CreateElement("head");
                doc.DocumentNode.PrependChild(head);
            }

            // ───────────────────────────────────────────────
            // 2. Формируем CSS-правила
            // ───────────────────────────────────────────────
            var cssBuilder = new StringBuilder();

            // 2.1. Базовый размер шрифта
            if (profile.HtmlFontSize > 0)
                cssBuilder.AppendLine($"body {{ font-size: {profile.HtmlFontSize}px; }}");

            // 2.2. Margin-ы для h1-h5
            string BuildMargin(ElementSpacingInfo info)
                => info == null
                    ? null
                    : $"{info.Top}px {info.Right}px {info.Bottom}px {info.Left}px";

            void AppendHeadingMargins(string selector, ElementSpacingInfo spacing)
            {
                var margin = BuildMargin(spacing);
                if (margin != null)
                    cssBuilder.AppendLine($"{selector} {{ margin: {margin}; }}");
            }

            AppendHeadingMargins("h1", profile.HtmlH1Margins);
            AppendHeadingMargins("h2", profile.HtmlH2Margins);
            AppendHeadingMargins("h3", profile.HtmlH3Margins);
            AppendHeadingMargins("h4", profile.HtmlH4Margins);
            AppendHeadingMargins("h5", profile.HtmlH5Margins);

            if (cssBuilder.Length == 0)
                return rawHtml;

            // ───────────────────────────────────────────────
            // 3. Добавляем (или обновляем) узел <style id="font-adjust">
            // ───────────────────────────────────────────────
            const string styleId = "font-adjust";
            var existingStyle = head.SelectSingleNode($"./style[@id='{styleId}']");
            existingStyle?.Remove();

            var styleNode = doc.CreateElement("style");
            styleNode.SetAttributeValue("id", styleId);
            styleNode.InnerHtml = cssBuilder.ToString();
            head.AppendChild(styleNode);

            // ───────────────────────────────────────────────
            // 4. Отдаём готовый HTML
            // ───────────────────────────────────────────────
            return doc.DocumentNode.OuterHtml;
        }


    }
}
