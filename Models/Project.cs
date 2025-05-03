using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Models
{
    public class Project
    {
        public string Name { get; set; } = "New project";
        public Settings Settings { get; set; } = new Settings();
        public ProjectData ProjectData { get; set; } = new ProjectData();
        public List<Guid> OpenedTabs { get; set; } = new List<Guid>();

        public Project()
        {
            Name = "New project";
        }

        public string GetNewTextPartName()
        {
            // Собираем все текущие названия (без учёта регистра, чтобы "TextPart-001" и "textpart-001"
            // считались занятыми одинаково)
            var used = new HashSet<string>(
                ProjectData.TextParts
                           .Select(tp => tp.Title ?? string.Empty),
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
