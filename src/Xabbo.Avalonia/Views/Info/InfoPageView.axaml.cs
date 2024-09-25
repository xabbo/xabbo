using System;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Xabbo.Avalonia.Views;

public partial class InfoPageView : UserControl
{
    public ReactiveCommand<string, Task> OpenUrlCmd { get; }

    public InfoPageView()
    {
        OpenUrlCmd = ReactiveCommand.Create<string, Task>(OpenUrl);

        InitializeComponent();
    }

    public async Task OpenUrl(string url)
    {
        if (TopLevel.GetTopLevel(this) is { Launcher: ILauncher launcher })
        {
            await launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
