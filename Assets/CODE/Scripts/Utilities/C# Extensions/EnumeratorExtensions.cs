using System.Collections.Generic;

namespace Utilities.Extensions
{
    public static class EnumeratorExtensions
    {
        /// Converts an IEnumerator<T> to an IEnumerable<T>.
        /// <param name="e">An instance of IEnumerator<T>.</param>
        /// <returns>An IEnumerable<T> with the same elements as the input instance.</returns>    
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> e)
        {
            while (e.MoveNext())
            {
                yield return e.Current;
            }
        }
    }
}