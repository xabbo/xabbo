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
}