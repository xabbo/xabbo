using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Xabbo.ViewModels;

namespace Xabbo.Views;

public partial class ChatPage : UserControl
{
    private IDisposable? _watchText;

    [Reactive] public bool IsAutoScrollEnabled { get; set; }

    public ChatPage()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_watchText is not null) return;

        if (DataContext is ChatPageViewModel chatPage)
        {
            _watchText = chatPage
                .WhenAnyValue(x => x.LogText)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => {
                    if (IsAutoScrollEnabled)
                        ChatScrollViewer.ScrollToEnd();
                });
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _watchText?.Dispose();
    }
}
