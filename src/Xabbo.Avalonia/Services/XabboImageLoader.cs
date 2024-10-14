using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncImageLoader.Loaders;
using Avalonia.Media.Imaging;

namespace Xabbo.Avalonia.Services;

public sealed class XabboImageLoader(HttpClient httpClient, bool disposeHttpClient)
    : RamCachedWebImageLoader(httpClient, disposeHttpClient)
{
    public static XabboImageLoader Instance { get; }

    private readonly ConcurrentDictionary<string, DateTime> _failureCache = [];

    static XabboImageLoader()
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "xabbo");
        Instance = new XabboImageLoader(client, false);
    }

    protected override Task<Bitmap?> LoadAsync(string url)
    {
        return base.LoadAsync(url);
    }

    public override async Task<Bitmap?> ProvideImageAsync(string url)
    {
        try
        {
            if (_failureCache.TryGetValue(url, out DateTime failureTime) &&
                (DateTime.Now - failureTime).TotalHours < 1)
            {
                return null;
            }

            Bitmap? image = await base.ProvideImageAsync(url);
            if (image is null)
            {
                _failureCache.AddOrUpdate(url, DateTime.Now, (_, _) => DateTime.Now);
            }

            return image;
        }
        catch
        {
            _failureCache.AddOrUpdate(url, DateTime.Now, (_, _) => DateTime.Now);
            throw;
        }
    }
}
