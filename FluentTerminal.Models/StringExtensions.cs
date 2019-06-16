namespace FluentTerminal.Models
{
    public static class StringExtensions
    {
        /// <summary>
        /// Compares two strings for equality, but assumes that null string is equal to an empty string.
        /// </summary>
        public static bool NullableEqualTo(this string original, string other) => string.IsNullOrEmpty(original)
            ? string.IsNullOrEmpty(other)
            : original.Equals(other);
    }
}