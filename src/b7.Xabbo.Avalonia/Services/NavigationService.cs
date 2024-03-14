using System;
using FluentAvalonia.UI.Controls;

namespace b7.Xabbo.Avalonia.Services;

public class NavigationService
{
    private Frame? _frame;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public void Navigate(Type type)
    {
        _frame?.Navigate(type);
    }



}
