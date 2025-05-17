using DocCreator01.Contracts;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
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

        /// <summary>Папка, куда складываются готовые документы.</summary>
        public string OutputDirectory => _appPathsHelper.DocumentsOutputDirectory;

        /// <inheritdoc />
        public string CreateDocument(IEnumerable<TextPart> textParts, string outputFileName, HtmlGenerationProfile profile = null)
        {
            if (textParts is null) throw new ArgumentNullException(nameof(textParts));
            if (string.IsNullOrWhiteSpace(outputFileName))
                throw new ArgumentException("Имя файла обязательно.", nameof(outputFileName));

            // Display message box showing the selected profile
            if (profile != null)
            {
                MessageBox.Show($"Используется профиль = {profile.Name}", 
                    "HTML Generation Profile", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }

            Directory.CreateDirectory(OutputDirectory);
            string outputFilePath = Path.Combine(OutputDirectory, outputFileName);
            string html = "";
            // Построение HTML
            html = BuildHtml(textParts);
            html = ToCenteredDocument(html);
            //html = FormatTables(html,(5,5,5,5));
            html = FormatTablesSlim(html, (5, 5, 5, 5));
            
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

        /// <summary>
        /// Приводит «растянутый» HTML к читаемому виду «документа в центре страницы».
        /// </summary>
        public static string ToCenteredDocument(string rawHtml)
        {
            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };
            doc.LoadHtml(rawHtml);

            // 1. Гармонично добавляем/находим <head>
            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head == null)
            {
                head = doc.CreateElement("head");
                doc.DocumentNode.PrependChild(head);
            }

            // 2. Общий стиль + «лист» документа
            const string readerCss = @"
html,body {margin:0;padding:0;height:100%;background:#f2f2f2;font-family:Arial, sans-serif;line-height:1.6;}
body      {padding:40px 0;}                    /* серые поля сверху/снизу              */
.page     {background:#fff;width:800px;        /* ≈ 210 мм / A4                        */
           margin:0 auto;padding:40px 60px;
           border:1px solid #ddd;
           box-shadow:0 0 10px rgba(0,0,0,.15);}
h1,h2,h3  {color:#333366;}";

            var style = doc.CreateElement("style");
            style.InnerHtml = readerCss;
            head.AppendChild(style);

            // 3. Оборачиваем содержимое <body> в .page
            var body = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode.AppendChild(doc.CreateElement("body"));

            var wrapper = doc.CreateElement("div");
            wrapper.SetAttributeValue("class", "page");

            // переносим ВСЕ существующие узлы внутрь нового контейнера
            foreach (var node in body.ChildNodes.ToList())
                wrapper.AppendChild(node);

            body.RemoveAllChildren();
            body.AppendChild(wrapper);

            // 4. Возвращаем готовый HTML
            return doc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// Приводит все таблицы к «табличному» виду: рамки, заголовок, настраиваемые отступы.
        /// </summary>
        /// <param name="rawHtml">Исходный HTML-текст</param>
        /// <param name="padding">Отступы ячейки: (top, right, bottom, left) в пикселях</param>
        /// <returns>Преобразованный HTML-текст</returns>
        public static string FormatTables(string rawHtml, (int top, int right, int bottom, int left) padding)
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

    }
}
