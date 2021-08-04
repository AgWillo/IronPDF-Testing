using System.Collections.Generic;
using System.Globalization;

namespace IronPDF_Issue
{
    public static class Extensions
    {
        /// <summary>
        /// Enumerates the text elements of a string ("normal" characters, surrogate pairs, and combining sequences)
        /// </summary>
        /// <param name="s">The <c>string</c> whose text elements are sought</param>
        /// <returns>A sequence of contiguous substrings of the supplied string, each being a single Unicode character,
        /// a surrogate pair, or a combining sequence</returns>
        public static IEnumerable<string> TextElements(this string s)
        {
            TextElementEnumerator stringEnumerator = StringInfo.GetTextElementEnumerator(s);
            while (stringEnumerator.MoveNext()) yield return stringEnumerator.GetTextElement();
        }
    }
}
