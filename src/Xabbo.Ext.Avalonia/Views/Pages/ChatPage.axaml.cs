using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Xabbo.Ext.Avalonia.ViewModels;
using ReactiveUI;

namespace Xabbo.Ext.Avalonia.Views;

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
