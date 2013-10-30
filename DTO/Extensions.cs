using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Outercurve.DTO
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        public static bool IsNullOrEmpty<T, TValue>(this IDictionary<T, TValue> dictionary)
        {
            return dictionary == null || !dictionary.Keys.Any();
        }

        // ReSharper disable InconsistentNaming
        /// <summary>
        ///     Formats the specified format string.
        /// </summary>
        /// <param name="formatString"> The format string. </param>
        /// <param name="args"> The args. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string format(this string formatString, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return formatString;
            }

            try
            {
                return String.Format(formatString, args);
            }
            catch (Exception e)
            {
                return formatString.Replace('{', '[').Replace('}', ']');
            }
        }

        // ReSharper restore InconsistentNaming
    }
}
