using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace b7.Xabbo.Services
{
    public class HabboUriProvider : IUriProvider<HabboEndpoints>
    {
        private readonly Dictionary<HabboEndpoints, string> _endpoints = new();

        public string Domain { get; set; }

        public Uri this[HabboEndpoints endpoint] => new(_endpoints[endpoint].Replace("{domain}", Domain));

        public HabboUriProvider(IConfiguration config)
        {
            Domain = config.GetValue<string>("Web:Domain");

            IConfigurationSection endpoints = config.GetSection("Web:Endpoints");
            foreach (IConfigurationSection endpointSection in endpoints.GetChildren())
            {
                string host = endpointSection.GetValue<string>("Host");

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
        }

        public Uri GetUri(HabboEndpoints endpoint, object? param = null, string? domain = null)
        {
            string url = _endpoints[endpoint].Replace("{domain}", domain ?? Domain);

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
}
