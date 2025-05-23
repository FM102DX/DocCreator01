using System;
using System.Collections.Generic;

public static class TextPartExtentions
{
	/// <summary>
	/// Перемещает элемент с позиции <paramref name="sourceIndex"/> на позицию <paramref name="destIndex"/>.
	/// </summary>
	public static void Move<T>(this IList<T> list, int sourceIndex, int destIndex)
	{
		if (list == null) throw new ArgumentNullException(nameof(list));
		if (sourceIndex == destIndex) return;
		if (sourceIndex < 0 || sourceIndex >= list.Count) throw new ArgumentOutOfRangeException(nameof(sourceIndex));
		if (destIndex   < 0 || destIndex   >= list.Count) throw new ArgumentOutOfRangeException(nameof(destIndex));

		T item = list[sourceIndex];
		list.RemoveAt(sourceIndex);

		// При удалении левее целевой позиции индекс смещается на 1.
		if (sourceIndex < destIndex) destIndex--;

		list.Insert(destIndex, item);
	}
}