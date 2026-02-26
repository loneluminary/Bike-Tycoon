using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Utilities.Extensions
{
	public static class DictionaryExtensions
	{
		/// Extension method for Dictionary that adds a new key-value pair or updates the value if the key exists.
		/// Return this Dictionary for method chaining.
		/// Arguments: TKey key: The key to add or update. TValue value: The value to set.
		public static Dictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			if (dictionary.ContainsKey(key)) dictionary[key] = value;
			else dictionary.Add(key, value);

			return dictionary;
		}

		/// Extension method for Dictionary that finds the key by its value.
		/// Returns TKey key found for the given value.
		/// Arguments: TValue value: The value to search for.
		public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
		{
			TKey key = default;
			foreach (var pair in dictionary.Where(pair => pair.Value.Equals(value)))
			{
				key = pair.Key;
				break;
			}

			return key;
		}
	
		public static TValue GetRandomValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null || dictionary.Count == 0) throw new InvalidOperationException("Cannot get a random value from an empty or null dictionary.");

			var random = new Random();
			int index = random.Next(dictionary.Count);
			return dictionary.ElementAt(index).Value;
		}
	
		public static TKey GetRandomKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null || dictionary.Count == 0) throw new InvalidOperationException("Cannot get a random key from an empty or null dictionary.");

			var random = new Random();
			int index = random.Next(dictionary.Count);
			return dictionary.ElementAt(index).Key;
		}
	
		public static (TKey Key, TValue Value) GetRandom<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null || dictionary.Count == 0) throw new InvalidOperationException("Cannot get a random key-value pair from an empty or null dictionary.");

			var random = new Random();
			int index = random.Next(dictionary.Count);
			var element = dictionary.ElementAt(index);
			return (element.Key, element.Value);
		}
	
		public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
		{
			foreach (var kvp in keyValuePairs)
			{
				dictionary[kvp.Key] = kvp.Value;
			}
		}

		/// Extension method for Dictionary that checks if a key exists and its value is not null.
		/// Returns bool indicating if the key exists and value is not null.
		/// Arguments: TKey key: The key to check.
		public static bool ContainsAndNotNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : Object => dictionary.ContainsKey(key) && dictionary[key] != null;
	}
}