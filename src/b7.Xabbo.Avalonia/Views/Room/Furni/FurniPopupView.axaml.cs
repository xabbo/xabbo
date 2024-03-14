using Avalonia;
using Avalonia.Controls;

namespace b7.Xabbo.Avalonia.Views;

public partial class FurniPopupView : UserControl
{
    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<FurniPopupView, bool>(nameof(ShowIcon), defaultValue: true);

    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public FurniPopupView()
    {
        InitializeComponent();
    }
}
