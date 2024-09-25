using System;
using Avalonia.Controls;

using Xabbo.ViewModels;

namespace Xabbo.Avalonia.Views;

public partial class GiftView : UserControl
{
    public GiftView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GiftViewModel gift)
        {
            GiftContents.Classes.Set("fadein", !gift.IsPeeking);
        }
    }
}