using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace b7.Xabbo.Services
{
    public class HabboUriProvider : IUriProvider<HabboEndpoints>
    {
        private readonly Dictionary<HabboEndpoints, Uri> _endpoints = new();

        public string Domain { get; set; }

        public Uri this[HabboEndpoints endpoint] => _endpoints[endpoint];

        public HabboUriProvider(IConfiguration config)
        {
            Domain = config.GetValue<string>("Web:Domain");

            IConfigurationSection endpoints = config.GetSection("Web:Endpoints");
            foreach (IConfigurationSection endpointSection in endpoints.GetChildren())
            {
                string host = endpointSection.GetValue<string>("Host");
                Uri baseUri = new(host);

                foreach (IConfigurationSection pathSection in endpointSection.GetSection("Paths").GetChildren())
                {
                    string endpointName = pathSection.Key;
                    
                    if (!Enum.TryParse(endpointName, out HabboEndpoints endpoint))
                    {
                        throw new Exception($"Unknown Habbo endpoint name: '{endpointName}'.");
                    }

                    string relativePath = pathSection.Value;
                    _endpoints[endpoint] = new Uri(baseUri, relativePath);
                }
            }
        }

        public Uri GetUri(HabboEndpoints endpoint, object? param = null, string? domain = null)
        {
            string uriString = _endpoints[endpoint].OriginalString
                .Replace("{domain}", domain ?? Domain);

            if (param is not null)
            {
                Type type = param.GetType();
                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    string selector = $"{{{propertyInfo.Name}}}";
                    if (!uriString.Contains(selector)) continue;

                    string propertyValue = propertyInfo.GetValue(param)?.ToString()
                        ?? throw new InvalidOperationException($"Value for property '{propertyInfo.Name}' was null.");

                    uriString = uriString.Replace(selector, WebUtility.UrlEncode(propertyValue));
                }
            }

            return new Uri(uriString);
        }
    }
}
