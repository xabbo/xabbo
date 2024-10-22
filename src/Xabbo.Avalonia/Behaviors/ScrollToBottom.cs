using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using ReactiveUI;

namespace Xabbo.Avalonia.Behaviors;

public class ScrollToBottom : Behavior<TemplatedControl>
{
    public static readonly AttachedProperty<bool> IsScrolledToBottomProperty
        = AvaloniaProperty.RegisterAttached<ScrollToBottom, Control, bool>(
            "IsScrolledToBottom", defaultValue: true);

    public static readonly AttachedProperty<ICommand?> CommandProperty
        = AvaloniaProperty.RegisterAttached<ScrollToBottom, Control, ICommand?>("Command");

    public static void SetIsScrolledToBottom(AvaloniaObject o, bool value)
    {
        o.SetValue(IsScrolledToBottomProperty, value);
    }

    public static bool GetIsScrolledToBottom(AvaloniaObject o)
    {
        return o.GetValue(IsScrolledToBottomProperty);
    }

    public static ICommand? GetCommand(AvaloniaObject o)
    {
        return o.GetValue(CommandProperty);
    }

    public static void SetCommand(AvaloniaObject o, ICommand? command)
    {
        o.SetValue(CommandProperty, command);
    }

    private readonly ICommand _scrollToEndCommand;
    private ScrollViewer? _scrollViewer;

    public ScrollToBottom()
    {
        _scrollToEndCommand = ReactiveCommand.Create(OnScrollToEnd);
    }

    private void OnScrollToEnd()
    {
        _scrollViewer?.ScrollToEnd();
    }

    [RequiresUnreferencedCode("override: This functionality is not compatible with trimming.")]
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            SetCommand(AssociatedObject, _scrollToEndCommand);

        if (AssociatedObject is ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;
            _scrollViewer.ScrollChanged += OnScrollChanged;
        }
        else if (AssociatedObject is TemplatedControl templatedControl)
        {
            templatedControl.TemplateApplied += AssociatedObjectOnTemplateApplied;
        }
    }

    private void AssociatedObjectOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (AssociatedObject is TemplatedControl control)
        {
            foreach (var descendant in AssociatedObject.GetSelfAndVisualDescendants())
            {
                if (descendant is ScrollViewer sv)
                {
                    _scrollViewer = sv;
                    break;
                }
            }
        }

        if (_scrollViewer is not null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer is { } sv)
        {
            AssociatedObject?.SetValue(IsScrolledToBottomProperty, (sv.Offset.Y + sv.Viewport.Height) == sv.Extent.Height);
        }
    }
}