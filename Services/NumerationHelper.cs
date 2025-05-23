using DocCreator01.Contracts;
using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    /// <summary>
    /// Нумерует элементы прямо в переданной коллекции.
    /// </summary>
    public static class NumerationHelper
    {
        /// <summary>
        /// Проставляет Order и ParagraphNo для всех элементов коллекции.
        ///   – верхним уровнем считается минимальный Level в коллекции;
        ///   – отсутствующие промежуточные уровни получают номер «1».
        /// </summary>
        public static void ApplyNumeration<T>(List<T> parts) where T : INumerableTextPart
        {
            if (parts == null || parts.Count == 0) return;

            /* 1. какой Level является корневым? */
            int rootLevel = parts.Min(p => Math.Max(0, p.Level));

            /* 2. счётчики для каждого относительного уровня (root == 0) */
            var counters = new List<int>();                // 0-й, 1-й, 2-й …

            foreach (var part in parts)
            {
                int levelAbs = Math.Max(0, part.Level);    // отрицательных не допускаем
                int levelRel = levelAbs - rootLevel;       // уровень относительно корня

                /* 2.1  расширяем список счётчиков до нужной глубины */
                while (counters.Count <= levelRel)
                    counters.Add(0);

                /* 2.2  обнуляем все уровни глубже текущего */
                for (int i = levelRel + 1; i < counters.Count; i++)
                    counters[i] = 0;

                /* 2.3  увеличиваем счётчик своего уровня                 */
                part.Order = ++counters[levelRel];

                /* 2.4  «заполняем дырки» — все пустые (0) уровни делаем 1 */
                for (int i = 0; i <= levelRel; i++)
                    if (counters[i] == 0) counters[i] = 1;

                /* 2.5  формируем ParagraphNo                              */
                part.ParagraphNo = string.Join(".", counters.Take(levelRel + 1));
            }
        }
    }
}