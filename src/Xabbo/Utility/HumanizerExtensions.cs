using System.Collections.Generic;
using System.Linq;

using Humanizer;

internal static class HumanizerExtensions
{
    public static string Humanize(this IEnumerable<string?> strings, int limit, string more)
    {
        string[] array = strings
            .Select(s => s?.Trim() ?? "")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        if (array.Length > limit)
        {
            return array
                .Take(limit)
                .Concat([more.ToQuantity(array.Length - limit)])
                .Humanize("and");
        }
        else
        {
            return array.Humanize("and");
        }
    }
}