using System;

namespace Utilities.Extensions
{
	public static class DecimalExtensions
	{
		/// Extension method for decimal that truncates the number to a specific number of decimal places.
		/// Returns decimal value truncated to the given digits.
		/// uint digits: Number of decimal places to keep.
		public static decimal TruncateTo(this decimal n, uint digits)
		{
			var wholePart = decimal.Truncate(n);

			var decimalPart = n - wholePart;
			var factor = checked(Math.Pow(10, digits));
			var decimalPartTruncated =
				Math.Truncate(decimalPart * (decimal)factor) / (decimal)factor;

			return wholePart + decimalPartTruncated;
		}
	}
}