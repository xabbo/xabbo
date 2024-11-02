using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Avalonia.Controls;
using Xabbo.Avalonia.Views;
using Xabbo.Services.Abstractions;

namespace Xabbo.Avalonia.Services;

public class LauncherService(Lazy<MainWindow> mainWindow) : ILauncherService
{
    private readonly Lazy<MainWindow> _mainWindow = mainWindow;

    public void Launch(string url, Dictionary<string, List<string>>? query = null)
    {
        var sb = new StringBuilder();

        sb.Append(url);

        bool first = true;
        if (query is not null)
        {
            foreach (var (key, values) in query)
            {
                foreach (var value in values)
                {
                    if (!first)
                        sb.Append('&');
                    else
                        sb.Append('?');
                    first = false;
                    sb.Append(key);
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode(value));
                }
            }
        }

        TopLevel.GetTopLevel(_mainWindow.Value)?.Launcher.LaunchUriAsync(new Uri(sb.ToString()));
    }
}