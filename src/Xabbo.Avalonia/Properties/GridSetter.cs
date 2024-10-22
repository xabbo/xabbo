using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace Xabbo.Avalonia.Properties;

public class GridSetter : AvaloniaObject
{
    public static readonly AttachedProperty<ColumnDefinitions> ColumnDefinitionsProperty =
        AvaloniaProperty.RegisterAttached<GridSetter, ColumnDefinitions>(nameof(ColumnDefinitions), typeof(GridSetter));

    public static readonly AttachedProperty<RowDefinitions> RowDefinitionsProperty =
        AvaloniaProperty.RegisterAttached<GridSetter, RowDefinitions>(nameof(RowDefinitions), typeof(GridSetter));

    public static void SetColumnDefinitions(AvaloniaObject element, ColumnDefinitions value) =>
        element.SetValue(ColumnDefinitionsProperty, value);

    public static ColumnDefinitions GetColumnDefinitions(AvaloniaObject element) =>
        element.GetValue(ColumnDefinitionsProperty);

    static GridSetter()
    {
        ColumnDefinitionsProperty.Changed.AddClassHandler<Grid, ColumnDefinitions>((grid, e) =>
        {
            grid.ColumnDefinitions.Clear();

            if (e.NewValue.GetValueOrDefault() is ColumnDefinitions columns)
            {
                grid.ColumnDefinitions.AddRange(columns.Select(x => new ColumnDefinition()
                {
                    Width = x.Width,
                    SharedSizeGroup = x.SharedSizeGroup,
                }));
            }
        });

        RowDefinitionsProperty.Changed.AddClassHandler<Grid, RowDefinitions>((grid, e) =>
        {
            grid.RowDefinitions.Clear();

            if (e.NewValue.GetValueOrDefault() is RowDefinitions rows)
            {
                grid.RowDefinitions.AddRange(rows.Select(x => new RowDefinition()
                {
                    Height = x.Height,
                    SharedSizeGroup = x.SharedSizeGroup,
                }));
            }
        });
    }
}