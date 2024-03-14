using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using b7.Xabbo.Avalonia.ViewModels;
using ReactiveUI;

namespace b7.Xabbo.Avalonia.Views;

public partial class ChatPage : UserControl
{
    public ChatPage()
    {
        InitializeComponent();

        ChatScrollViewer.ScrollChanged += ChatScrollViewer_ScrollChanged;
    }

    private void ChatScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        Console.WriteLine(e.OffsetDelta);
        Console.WriteLine(e.ViewportDelta);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ChatPageViewModel vm)
        {
        }
    }
}
