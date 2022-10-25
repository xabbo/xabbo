using System.Text.RegularExpressions;

namespace b7.Xabbo.Util;

public static class StringUtil
{
    /// <summary>
    /// Converts the specified wildcard pattern into a regular expression.
    /// </summary>
    /// <param name="pattern">The wildcard pattern.</param>
    /// <param name="options">The options to pass into the <see cref="Regex"/> constructor.</param>
    /// <param name="anchored">Whether the wildcard pattern should match the start and end of the string exactly.</param>
    /// <param name="useAnchors">
    /// Whether the <c>^</c> and <c>$</c> characters should be interpreted to match the start and end of the string respectively.
    /// If the <c>^</c> or <c>$</c> characters do not appear at their respective locations, they will be interpreted literally.
    /// This option has no effect if <paramref name="anchored"/> is <see langword="true"/>.
    /// </param>
    public static Regex CreateWildcardRegex(string pattern, RegexOptions options = RegexOptions.IgnoreCase,
        bool anchored = false, bool useAnchors = true)
    {
        pattern = Regex.Escape(pattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".");

        if (anchored)
        {
            pattern = $"^{pattern}$";
        }
        else if (useAnchors)
        {
            if (pattern.StartsWith(@"\^")) pattern = pattern[1..];
            if (pattern.EndsWith(@"\$")) pattern = pattern[..^2] + '$';
        }

        return new Regex(pattern, options);
    }
}
