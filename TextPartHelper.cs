using System;
using System.Collections.Generic;

public static class TextPartExtentions
{
    /// <summary>
    /// Перемещает элемент с позиции <paramref name="sourceIndex"/> на позицию <paramref name="destIndex"/>.
    /// </summary>
    public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex ||
            oldIndex < 0 || oldIndex >= list.Count ||
            newIndex < 0 || newIndex >= list.Count)
            return;

        T item = list[oldIndex];
        list.RemoveAt(oldIndex);   // после этого список стал короче на 1
        list.Insert(newIndex, item); // вставляем точно в newIndex
    }
}