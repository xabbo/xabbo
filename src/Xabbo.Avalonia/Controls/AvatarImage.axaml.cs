using Avalonia;
using Avalonia.Controls.Primitives;
using Xabbo.Utility;

namespace Xabbo.Avalonia.Controls;

public class AvatarImage : TemplatedControl
{
    public static readonly StyledProperty<string?> FigureStringProperty =
        AvaloniaProperty.Register<AvatarImage, string?>(nameof(FigureString));

    public static readonly StyledProperty<int> DirectionProperty =
        AvaloniaProperty.Register<AvatarImage, int>(nameof(Direction), defaultValue: 2);

    public static readonly StyledProperty<bool> HeadOnlyProperty =
        AvaloniaProperty.Register<AvatarImage, bool>(nameof(HeadOnly));

    public static readonly DirectProperty<AvatarImage, string> PlaceholderProperty =
        AvaloniaProperty.RegisterDirect<AvatarImage, string>(nameof(Placeholder),
            x => x.Placeholder);

    public static readonly DirectProperty<AvatarImage, string?> AvatarImageUrlProperty =
        AvaloniaProperty.RegisterDirect<AvatarImage, string?>(nameof(AvatarImageUrl),
            x => x.AvatarImageUrl);

    public string? FigureString
    {
        get => GetValue(FigureStringProperty);
        set => SetValue(FigureStringProperty, value);
    }

    public int Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public bool HeadOnly
    {
        get => GetValue(HeadOnlyProperty);
        set => SetValue(HeadOnlyProperty, value);
    }

    private string _placeholder = "avares://Xabbo.Avalonia/Assets/Images/Avatar/body-2.png";
    public string Placeholder
    {
        get => _placeholder;
        private set => SetAndRaise(PlaceholderProperty, ref _placeholder, value);
    }

    private string? _avatarImageUrl;
    public string? AvatarImageUrl
    {
        get => _avatarImageUrl;
        private set => SetAndRaise(AvatarImageUrlProperty, ref _avatarImageUrl, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name is nameof(FigureString) or nameof(Direction) or nameof(HeadOnly))
        {
            Placeholder = $"avares://Xabbo.Avalonia/Assets/Images/Avatar/{(HeadOnly ? "head" : "body")}-{Direction}.png";

            if (string.IsNullOrWhiteSpace(FigureString))
            {
                AvatarImageUrl = null;
            }
            else
            {
                AvatarImageUrl = UrlHelper.AvatarImageUrl(
                    figure: FigureString,
                    direction: Direction,
                    headOnly: HeadOnly
                );
            }
        }
    }
}