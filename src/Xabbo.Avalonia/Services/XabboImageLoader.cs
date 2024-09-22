using System.Net.Http;
using AsyncImageLoader.Loaders;

namespace Xabbo.Avalonia.Services;

public sealed class XabboImageLoader(HttpClient httpClient, bool disposeHttpClient)
    : RamCachedWebImageLoader(httpClient, disposeHttpClient)
{
    public static XabboImageLoader Instance { get; }

    static XabboImageLoader()
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "xabbo");
        Instance = new XabboImageLoader(client, false);
    }
}
