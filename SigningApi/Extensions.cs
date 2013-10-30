using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Outercurve.SigningApi
{
    internal static class Extensions
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Formats the specified format string.
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
            catch (Exception)
            {
                return formatString.Replace('{', '[').Replace('}', ']');
            }
        }
    }
}