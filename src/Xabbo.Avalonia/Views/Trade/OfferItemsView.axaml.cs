using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace Xabbo.Avalonia.Views;

public partial class OfferItemsView : UserControl
{
    public OfferItemsView()
    {
        InitializeComponent();
    }

    private void OnValueChanged(NumberBox box, NumberBoxValueChangedEventArgs e)
    {
        Console.WriteLine($"{e.OldValue} -> {e.NewValue}");
    }
}