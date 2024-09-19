using System;

using Avalonia.Controls;

namespace Xabbo.Views;

public partial class ChatPage : UserControl
{
    public ChatPage()
    {
        InitializeComponent();

        ChatScrollViewer.ScrollChanged += ChatScrollViewer_ScrollChanged;
    }

    private void ChatScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
    }
}
