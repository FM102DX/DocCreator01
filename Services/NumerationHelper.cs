using DocCreator01.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public static class NumerationHelper
    {
        /// <summary>
        /// Пронумеровывает все элементы коллекции.
        /// Коллекция передаётся по ссылке, поэтому возвращать её не нужно.
        /// </summary>
        public static void ApplyNumeration(
            ObservableCollection<INumerableTextPart> parts)
        {
            if (parts == null) return;

            // levelCounters[i] хранит текущий порядковый номер для уровня i
            var levelCounters = new List<int>();

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                int level = part.Level < 0 ? 0 : part.Level;

                // гарантируем нужную длину списка счётчиков
                while (levelCounters.Count <= level)
                    levelCounters.Add(0);

                // обнуляем все уровни глубже текущего
                for (int j = level + 1; j < levelCounters.Count; j++)
                    levelCounters[j] = 0;

                // увеличиваем счётчик текущего уровня
                levelCounters[level]++;

                // ORDER = порядковый номер в пределах своего уровня
                part.Order = levelCounters[level];

                // собираем ParagraphNo: 1.2.3 ...
                var sb = new StringBuilder();
                for (int l = 0; l <= level; l++)
                {
                    if (levelCounters[l] == 0) break;      // на всякий случай
                    if (sb.Length > 0) sb.Append('.');
                    sb.Append(levelCounters[l]);
                }
                part.ParagraphNo = sb.ToString();
            }
        }
    }
}
