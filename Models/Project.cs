using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocCreator01.Models
{
    public class Project
    {
        public string Name { get; set; } = "New project";
        public Settings Settings { get; set; } = new Settings();
        public ProjectData ProjectData { get; set; } = new ProjectData();
        public List<Guid> OpenedTabs { get; set; } = new List<Guid>();
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets the folder path where the project is located (extracted from FilePath)
        /// </summary>
        [JsonIgnore]
        public string ProjectFolder => string.IsNullOrEmpty(FilePath) ? string.Empty : Path.GetDirectoryName(FilePath);

        public Project()
        {
        }

        public string GetNewTextPartName()
        {
            // Собираем все текущие названия (без учёта регистра, чтобы "TextPart-001" и "textpart-001"
            // считались занятыми одинаково)
            var used = new HashSet<string>(
                ProjectData.TextParts
                           .Select(tp => tp.Name ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);

            // Ищем свободный суффикс от 001 до 999
            for (int i = 1; i <= 999; i++)
            {
                var candidate = $"TextPart-{i:000}";
                if (!used.Contains(candidate))
                    return candidate;
            }

            // Если все имена заняты – сообщаем об этом вызывающему коду
            throw new InvalidOperationException(
                "Свободных имён вида \"TextPart-###\" не осталось (1-999 заняты).");
        }
    }
}
