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
        public static class NumerationHelper
        {
            /// <summary>
            /// Нумерует элементы прямо в переданной коллекции.
            /// </summary>
            public static void ApplyNumeration(ObservableCollection<INumerableTextPart> parts)
            {
                if (parts == null || parts.Count == 0) return;

                const int MaxDepth = 32; // при необходимости увеличьте
                var counters = new int[MaxDepth]; // индекс = уровень, значение = счётчик

                foreach (var part in parts)
                {
                    int level = part.Level < 0 ? 0 : part.Level;
                    if (level >= MaxDepth)
                        throw new ArgumentOutOfRangeException(nameof(part.Level),
                            $"Level {level} превышает допустимый предел {MaxDepth - 1}");

                    // 1) сбрасываем счётчики более глубоких уровней
                    for (int d = level + 1; d < MaxDepth; d++)
                        counters[d] = 0;

                    // 2) инкрементируем счётчик своего уровня
                    counters[level]++;

                    // 3) присваиваем Order
                    part.Order = counters[level];

                    // 4) формируем ParagraphNo:
                    //    берём все ненулевые значения от 0 до level и соединяем через '.'
                    part.ParagraphNo = string.Join(".",
                        counters.Take(level + 1).Where(n => n > 0));
                }
            }
        }
    }