using System;
using System.Collections.Generic;
using System.Linq;

namespace DocCreator01.Models
{
    public static class ProjectExtensions
    {
        public static string GetNewTextPartName(this Project project)
        {
            // Собираем все текущие названия (без учёта регистра, чтобы "TextPart-001" и "textpart-001"
            // считались занятыми одинаково)
            var used = new HashSet<string>(
                project.ProjectData.TextParts
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
