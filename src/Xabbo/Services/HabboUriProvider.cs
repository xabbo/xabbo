using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;

using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public class HabboUriProvider : IUriProvider<HabboEndpoints>
{
    private readonly Dictionary<HabboEndpoints, string> _endpoints = [];

    public string Host { get; set; } = "";

    public Uri this[HabboEndpoints endpoint] => GetUri(endpoint);

    public HabboUriProvider(IConfiguration config)
    {
        IConfigurationSection endpointSection = config.GetSection("Web:Endpoints");
        string? host = endpointSection.GetValue<string>("Host");

        foreach (IConfigurationSection pathSection in endpointSection.GetSection("Paths").GetChildren())
        {
            string endpointName = pathSection.Key;

            if (!Enum.TryParse(endpointName, out HabboEndpoints endpoint))
            {
                throw new Exception($"Unknown Habbo endpoint name: '{endpointName}'.");
            }

            _endpoints[endpoint] = host + pathSection.Value;
        }
    }

    public Uri GetUri(HabboEndpoints endpoint, object? param = null)
    {
        string url = "https://" + Host + _endpoints[endpoint];

        if (param is not null)
        {
            Type type = param.GetType();
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                string selector = $"{{{propertyInfo.Name}}}";
                if (!url.Contains(selector)) continue;

                string propertyValue = propertyInfo.GetValue(param)?.ToString()
                    ?? throw new InvalidOperationException($"Value for property '{propertyInfo.Name}' was null.");

                url = url.Replace(selector, WebUtility.UrlEncode(propertyValue));
            }
        }

        return new Uri(url);
    }
}
