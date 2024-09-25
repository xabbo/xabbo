using System.Web;

namespace Xabbo.Utility;

public static class UrlHelper
{
    public static string? AvatarImageUrl(string? name = null, string? figure = null,
        int direction = 2, bool headOnly = false)
    {
        var query = HttpUtility.ParseQueryString("");
        query.Add("direction", direction.ToString());
        if (headOnly)
            query.Add("headonly", "1");
        if (string.IsNullOrWhiteSpace(figure))
            query.Add("user", name);
        else
            query.Add("figure", figure);

        return $"https://habbo.com/habbo-imaging/avatarimage?{query}";
    }

    public static string FurniIconUrl(string identifier, int revision)
    {
        identifier = identifier.Replace('*', '_');
        return $"https://images.habbo.com/dcr/hof_furni/{revision}/{identifier}_icon.png";
    }
}