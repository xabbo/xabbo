using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Xabbo.Avalonia.Behaviors;

public class NonEmptyNumericUpDownBehavior : Behavior<NumericUpDown>
{
    [RequiresUnreferencedCode("override: This functionality is not compatible with trimming.")]
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { } nud)
        {
            nud.ValueChanged += OnValueChanged;
        }
    }

    private void OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (AssociatedObject is { } nud &&
            e.OldValue is not null &&
            e.NewValue is null)
        {
            nud.Value = e.OldValue;
        }
    }
}