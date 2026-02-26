using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Utilities.Extensions
{
	public static class StringExtensions
	{
		public static T ToEnum<T>(this string str) where T : struct, Enum
		{
			if (Enum.TryParse<T>(str, true, out var result)) return result;

			throw new ArgumentException($"Cannot convert '{str}' to enum {typeof(T).Name}");
		}

		public static string Truncate(this string str, int maxLength)
		{
			if (string.IsNullOrEmpty(str)) return str;

			return str.Length <= maxLength ? str : str[..maxLength];
		}

		public static string ToTitleCase(this string str)
		{
			return string.IsNullOrEmpty(str) ? str : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
		}

		public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

		public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

		public static string Reverse(this string str)
		{
			if (string.IsNullOrEmpty(str)) return str;

			char[] charArray = str.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		public static string RemoveWhitespace(this string str) => new(str.Where(c => !char.IsWhiteSpace(c)).ToArray());

		public static string ToCamelCase(this string str)
		{
			if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
				return str;

			string camelCase = char.ToLower(str[0]).ToString();
			if (str.Length > 1)
				camelCase += str.Substring(1);
			return camelCase;
		}

		public static string SplitCamelCase(this string str)
		{
			return string.IsNullOrEmpty(str) ? str : Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
		}
	
		public static float TryParseFloat(this string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text = text.Replace(',', '.');
				string text2 = text;
				if (text2[0] == '$')
				{
					text2 = text2.Remove(0, 1);
				}
				int length = text2.Length;
				int num = 0;
				int num2 = 0;
				bool flag = false;
				for (int i = 0; i < length; i++)
				{
					if (text2[i] == '<')
					{
						flag = true;
						num = i;
					}
					if (text2[i] == '>')
					{
						num2 = i;
						break;
					}
				}
				if (flag)
				{
					text = text2.Remove(num, num2 - num + 1);
				}
				CultureInfo provider = new CultureInfo("en-US");
				const NumberStyles style = NumberStyles.Any;
				if (float.TryParse(text, style, provider, out var result))
				{
					return result;
				}
			}
		
			return 0f;
		}
		
		public static string ToCapital(this string input)
		{
			return input switch
			{
				null => throw new ArgumentNullException(nameof(input)),
				"" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
				_ => input[0].ToString().ToUpper() + input[1..]
			};
		}
		
		// Rich text formatting for Unity UI elements that support rich text.
		public static string RichColor(this string text, Color color) => $"<color={color.ToHex()}>{text}</color>";
		public static string RichSize(this string text, int size) => $"<size={size}>{text}</size>";
		public static string RichBold(this string text) => $"<b>{text}</b>";
		public static string RichItalic(this string text) => $"<i>{text}</i>";
		public static string RichUnderline(this string text) => $"<u>{text}</u>";
		public static string RichStrikethrough(this string text) => $"<s>{text}</s>";
		public static string RichFont(this string text, string font) => $"<font={font}>{text}</font>";
		public static string RichAlign(this string text, string align) => $"<align={align}>{text}</align>";
		public static string RichGradient(this string text, string color1, string color2) => $"<gradient={color1},{color2}>{text}</gradient>";
		public static string RichRotation(this string text, float angle) => $"<rotate={angle}>{text}</rotate>";
		public static string RichSpace(this string text, float space) => $"<space={space}>{text}</space>";

		private static string ToHex(this Color color)
		{
			return $"#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}";
		}

		private static byte ToByte(float f)
		{
			f = Mathf.Clamp01(f);
			return (byte)(f * 255f);
		}
		
		 /// Checks if a string contains null, empty or white space.
        public static bool IsBlank(this string val) => val.IsNullOrWhiteSpace() || val.IsNullOrEmpty();

        /// Checks if a string is null and returns an empty string if it is.
        public static string OrEmpty(this string val) => val ?? string.Empty;

        /// Shortens a string to the specified maximum length. If the string's length
        /// is less than the maxLength, the original string is returned.
        public static string Shorten(this string val, int maxLength)
        {
            if (val.IsBlank()) return val;
            return val.Length <= maxLength ? val : val.Substring(0, maxLength);
        }

        /// Slices a string from the start index to the end index.
        public static string Slice(this string val, int startIndex, int endIndex)
        {
            if (val.IsBlank())
            {
                throw new ArgumentNullException(nameof(val), "Value cannot be null or empty.");
            }

            if (startIndex < 0 || startIndex > val.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            // If the end index is negative, it will be counted from the end of the string.
            endIndex = endIndex < 0 ? val.Length + endIndex : endIndex;

            if (endIndex < 0 || endIndex < startIndex || endIndex > val.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(endIndex));
            }

            return val.Substring(startIndex, endIndex - startIndex);
        }

        /// Converts the input string to an alphanumeric string, optionally allowing periods.
        /// <param name="input">The input string to be converted.</param>
        /// <param name="allowPeriods">A boolean flag indicating whether periods should be allowed in the output string.</param>
        /// A new string containing only alphanumeric characters, underscores, and optionally periods.
        /// If the input string is null or empty, an empty string is returned.
        public static string ConvertToAlphanumeric(this string input, bool allowPeriods = false)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            List<char> filteredChars = new List<char>();
            int lastValidIndex = -1;

            // Iterate over the input string, filtering and determining valid start/end indices
            foreach (char character in input.Where(character => char.IsLetterOrDigit(character) || character == '_' || (allowPeriods && character == '.')).Where(character => filteredChars.Count != 0 || (!char.IsDigit(character) && character != '.')))
            {
                filteredChars.Add(character);
                lastValidIndex = filteredChars.Count - 1; // Update lastValidIndex for valid characters
            }

            // Remove trailing periods
            while (lastValidIndex >= 0 && filteredChars[lastValidIndex] == '.')
            {
                lastValidIndex--;
            }

            // Return the filtered string
            return lastValidIndex >= 0 ? new string(filteredChars.ToArray(), 0, lastValidIndex + 1) : string.Empty;
        }
	}
}