using System;
using System.Text.RegularExpressions;

namespace b7.Xabbo.Util
{
    public static class HabboUtil
    {
        public static string? GetDomainFromGameHost(string host)
        {
            Match m = Regex.Match(host, @"^game-(?<host>[a-z]+)\.habbo\.com$");
            if (!m.Success) return null;

            return m.Groups["host"].Value switch
            {
                "de" or "nl" or "fr" or "it" => host,
                "br" => "com.br",
                "tr" => "com.tr",
                "us" => "com",
                _ => null
            };
        }
    }
}
