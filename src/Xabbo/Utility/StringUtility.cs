using System.Reflection;
using System.Text.RegularExpressions;

namespace Xabbo.Utility;

public static class StringUtility
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

    public static string GetVersionString(this Assembly? assembly, bool includePrefix = true)
    {
        string? version = assembly?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly?.GetName().Version?.ToString(3);

        if (version is null)
            return "unknown version";

        if (version.StartsWith('v') && !includePrefix)
            version = version[1..];
        else if (!version.StartsWith('v') && includePrefix)
            version = "v" + version;

        int index = version.IndexOf('+');
        if (index > 0)
            version = version[..index];

        return version;
    }
}
