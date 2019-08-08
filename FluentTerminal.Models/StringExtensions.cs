using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FluentTerminal.Models
{
    public static class StringExtensions
    {
        private static readonly Regex RxNonWhiteSpace = new Regex(@"\S+", RegexOptions.Compiled);

        /// <summary>
        /// Compares two strings for equality, but assumes that null string is equal to an empty string.
        /// </summary>
        public static bool NullableEqualTo(this string original, string other,
            StringComparison stringComparison = StringComparison.Ordinal) => string.IsNullOrEmpty(original)
            ? string.IsNullOrEmpty(other)
            : original.Equals(other, stringComparison);

        /// <summary>
        /// Splits input text into words, by splitting at white-space characters. It doesn't take care about quotes.
        /// </summary>
        public static IEnumerable<string> SplitWords(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            foreach (Match match in RxNonWhiteSpace.Matches(text.Trim()))
            {
                yield return match.Value;
            }
        }
    }
}