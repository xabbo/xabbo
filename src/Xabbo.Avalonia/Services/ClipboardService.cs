using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

using Xabbo.Services.Abstractions;

namespace Xabbo.Avalonia.Services;

public class ClipboardService(Application app, IUiContext uiContext) : IClipboardService
{
    private readonly Application _app = app;
    private readonly IUiContext _uiContext = uiContext;

    public void SetText(string text)
    {
        if (_app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow.Clipboard: IClipboard clipboard })
        {
            clipboard.SetTextAsync(text);
        }
    }
}