using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View;

public partial class WardrobeView : UserControl
{
    private FigureViewModel? _dragStartFigure;

    public WardrobeView()
    {
        InitializeComponent();
    }

    private void ListItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragStartFigure is null) return;

        if (e.ChangedButton != MouseButton.Left ||
            Keyboard.Modifiers != ModifierKeys.None)
            return;

        FrameworkElement? element = sender as FrameworkElement;
        if (element?.DataContext is FigureViewModel figureView)
        {
            if (figureView != _dragStartFigure)
            {
                ((WardrobeViewManager)DataContext).MoveFigure(_dragStartFigure, figureView);
                figureItems.SelectedItem = _dragStartFigure;
            }
        }

        _dragStartFigure = null;
    }

    private void ListItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left ||
            Keyboard.Modifiers != ModifierKeys.None)
            return;

        var element = sender as FrameworkElement;
        _dragStartFigure = element?.DataContext as FigureViewModel;
    }
}
