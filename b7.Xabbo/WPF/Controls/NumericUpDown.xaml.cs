using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace b7.Xabbo.WPF.Controls;

public partial class NumericUpDown : UserControl
{
    #region - Dependency properties -
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(int),
        typeof(NumericUpDown),
        new FrameworkPropertyMetadata(
            0,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            new PropertyChangedCallback(OnValueChanged)
        )
    );

    private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        ((NumericUpDown)o).OnValueChanged((int)e.NewValue);
    }

    public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(
        "Interval",
        typeof(int),
        typeof(NumericUpDown),
        new FrameworkPropertyMetadata(
            100,
            FrameworkPropertyMetadataOptions.None,
            new PropertyChangedCallback(OnIntervalChanged)
        )
    );

    private static void OnIntervalChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        ((NumericUpDown)o).OnValueChanged((int)e.NewValue);
    }
    #endregion

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    public int Step { get; set; } = 1;

    public event EventHandler<RoutedEventArgs> Increment;
    public event EventHandler<RoutedEventArgs> Decrement;

    public NumericUpDown()
    {
        InitializeComponent();
    }

    private void OnValueChanged(int newValue)
    {
        Value = newValue;
    }

    private void OnIncrement(object sender, RoutedEventArgs e)
    {
        Increment?.Invoke(this, e);
        if (e.Handled) return;

        Value += Step;
    }

    private void OnDecrement(object sender, RoutedEventArgs e)
    {
        Decrement?.Invoke(this, e);
        if (e.Handled) return;

        Value -= Step;
    }

    private void TextBoxValue_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            OnIncrement(this, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            OnDecrement(this, e);
            e.Handled = true;
        }
    }
}
