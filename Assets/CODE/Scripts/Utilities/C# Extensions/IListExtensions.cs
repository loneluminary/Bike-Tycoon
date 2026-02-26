using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities.Extensions
{
	public static class IListExtensions
	{
		/// Checks if the list is null or contains no elements
		public static bool IsNullOrEmpty<T>(this IList<T> list) => list == null || list.Count == 0;

		/// Returns a random item from the list
		public static T GetRandom<T>(this IList<T> _array, int subtract = 0)
		{
			if (_array == null) throw new IndexOutOfRangeException(" [ array is null ] Cannot select a random item from null");
			if (_array.Count == 0) throw new IndexOutOfRangeException(" [ array is empty ] Cannot select a random item from an empty array");
			return _array[Random.Range(0, _array.Count - subtract)];
		}
		
		public static T GetRandomOfType<T>(this IList<T> _array, Type type)
		{
			var filteredList = _array.Where(arg => arg.GetType() == type);
			return filteredList.ToArray().GetRandom();
		}
		
		/// Gets a randoms item based on weights (useful for probability-based selections)
		public static T GetRandomByWeights<T>(this IList<T> list, IList<float> weights)
		{
			float totalWeight = weights.Sum();

			float random = Random.Range(0, totalWeight);
			float current = 0;

			for (int i = 0; i < list.Count; i++)
			{
				current += weights[i];
				if (random <= current)
					return list[i];
			}

			return list[^1];
		}

		/// Returns a specified number of random items from the list without duplicates
		public static List<T> GetUniqueRandoms<T>(this IList<T> list, int count, bool useDuplicatesIfNotInRange = true)
		{
			if (count > list.Count && useDuplicatesIfNotInRange)
			{
				Debug.LogWarning("Requested count is larger than list size, cannot return unique items returning all items instead.");
				return list.GetRandoms(count);
			}

			count.Clamp(1, list.Count);

			var result = new List<T>();
			var tempList = new List<T>(list);
			for (int i = 0; i < count; i++)
			{
				int index = Random.Range(0, tempList.Count);
				result.Add(tempList[index]);
				tempList.RemoveAt(index);
			}

			return result;
		}
			
		/// Returns a specified number of random items from the list with duplicates.
		public static List<T> GetRandoms<T>(this IList<T> list, int count)
		{
			var result = new List<T>();
			
			for (int i = 0; i < count; i++)
			{
				int index = Random.Range(0, list.Count);
				result.Add(list[index]);
			}

			return result;
		}

		/// Removes all null elements from the list
		public static IList<T> RemoveNulls<T>(this List<T> list) where T : class
		{
			list.RemoveAll(item => item == null);
			return list;
		}

		/// Randomly reorders the elements in the list using the Fisher-Yates shuffle algorithm
		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = Random.Range(0, n + 1);
				(list[k], list[n]) = (list[n], list[k]);
			}

			return list;
		}

		public static IList<T> ForEach<T>(this IList<T> list, Action<T, int> action)
		{
			for (int i = 0; i < list.Count; i++)
				action?.Invoke(list[i], i);
			return list;
		}

		public static IList<T> ForEach<T>(this IList<T> list, Action<T> action)
		{
			foreach (var t in list) action?.Invoke(t);

			return list;
		}

		/// Moves an item from one index to another
		public static IList<T> Move<T>(this IList<T> list, int oldIndex, int newIndex)
		{
			if (oldIndex == newIndex)
				return list;

			T item = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(newIndex, item);
			return list;
		}

		/// Swaps two items in the list
		public static IList<T> Swap<T>(this IList<T> list, int index1, int index2)
		{
			if (index1 == index2) return list;

			(list[index1], list[index2]) = (list[index2], list[index1]);
			return list;
		}

		/// Replaces the first occurrence of an item with another
		public static IList<T> Replace<T>(this IList<T> list, T oldItem, T newItem)
		{
			int index = list.IndexOf(oldItem);
			list[index] = newItem;
			return list;
		}

		/// Removes duplicate items from the list while preserving order
		public static IList<T> RemoveDuplicates<T>(this IList<T> list)
		{
			var seen = new HashSet<T>();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (!seen.Add(list[i])) list.RemoveAt(i);
			}

			return list;
		}
		
		public static List<T> TryRemove<T>(this List<T> list, T obj)
		{
			if (obj != null && list.Contains(obj)) list.Remove(obj);
			return list;
		}

		/// Pops element by <paramref name="index"/>.
		public static T Pop<T>(this IList<T> list, int? index = null)
		{
			index ??= list.Count - 1;
			var element = list[index.Value];
			list.RemoveAt(index.Value);

			return element;
		}

		/// Pops elements by <paramref name="indexes"/>.
		public static List<T> PopList<T>(this IList<T> list, params int[] indexes)
		{
			return indexes.Select(index => list.Pop(index)).ToList();
		}

		/// Pops random element from <paramref name="list"/>.
		public static T PopRandom<T>(this IList<T> list)
		{
			var index = Random.Range(0, list.Count);
			return list.Pop(index);
		}

		/// Pops random element from <paramref name="list"/>.
		public static (T element, int index) PopRandomTuple<T>(this IList<T> list)
		{
			var index = Random.Range(0, list.Count);
			return (list.Pop(index), index);
		}

		/// Pops random elements from list.
		public static List<T> PopRandoms<T>(this IList<T> list, int count)
		{
			var popped = new List<T>();

			for (int i = 0; i < count; i++)
				popped.Add(list.PopRandom());

			return popped;
		}

		/// Pops random elements from list.
		public static List<(T element, int index)> PopRandomsTupleList<T>(this IList<T> list, int count)
		{
			var popped = new List<(T element, int index)>();

			for (int i = 0; i < count; i++)
				popped.Add(list.PopRandomTuple());

			return popped;
		}

		public static void TryAdd<T>(this IList<T> list, T item)
		{
			if (!list.Contains(item)) list.Add(item);
		}
		
		/// Extension method for IList that removes elements from a starting index to the end.
		/// Return this IList<T> for method chaining.
		/// Arguments: int index: Starting index to remove from.
		public static IList<T> RemoveRange<T>(this IList<T> list, int index)
		{
			for (int i = list.Count - 1; i >= index; i++) list.RemoveAt(i);
			return list;
		}

		/// Extension method for IList that adds multiple items.
		/// Return this IList<T> for method chaining.
		/// Arguments: T[] items: Items to add.
		public static IList<T> AddMultiple<T>(this IList<T> list, params T[] items)
		{
			items.ForEach(item => list.Add(item));
			return list;
		}

		/// Extension method for IList that removes multiple items.
		/// Return this IList<T> for method chaining.
		/// Arguments: T[] items: Items to remove.
		public static IList<T> RemoveMultiple<T>(this IList<T> list, params T[] items)
		{
			items.ForEach(item => list.Remove(item));
			return list;
		}

		/// Extension method for IList that clears and adds multiple items.
		/// Return this IList<T> for method chaining.
		/// Arguments: T[] items: Items to set.
		// public static IList<T> SetMultiple<T>(this IList<T> list, params T[] items)
		// {
		//     if (list is T[] array)
		//     {
		//         list = new T[items.Length];

		//         for (int i = 0; i < array.Length; i++)
		//             array[i] = items[i];
		//     }
		//     else
		//     {
		//         if (list == null)
		//             list = new List<T>();
		//         else
		//             list.Clear();
		//         items.ForEach(item => list.Add(item));
		//     }
		//     return list;
		// }

		#region Json

		public static string ToJson<T>(this List<T> list)
		{
			var wrapper = new Wrapper<T> { Elements = list };
			return JsonUtility.ToJson(wrapper);
		}

		public static List<T> FromJson<T>(this List<T> list, string json)
		{
			if(string.IsNullOrEmpty(json)) return list;
			
			var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
			if (wrapper?.Elements != null)
			{
				list.Clear();
				list.AddRange(wrapper.Elements);
			}

			return list;
		}

		[Serializable]
		private class Wrapper<T>
		{
			public List<T> Elements;
		}

		#endregion
	}
}