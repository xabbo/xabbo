using System;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Xabbo.Abstractions;
using Xabbo.Utility;

namespace Xabbo.Avalonia.Controls;

public class FurniIconImage : TemplatedControl
{
    public static readonly DirectProperty<FurniIconImage, IItemIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<FurniIconImage, IItemIcon?>(
            nameof(Icon),
            x => x.Icon,
            (x, v) => x.Icon = v);

    public static readonly DirectProperty<FurniIconImage, string?> IconImageUrlProperty =
        AvaloniaProperty.RegisterDirect<FurniIconImage, string?>(nameof(IconImageUrl),
            x => x.IconImageUrl);

    public static readonly DirectProperty<FurniIconImage, IImage?> CurrentImageProperty =
        AvaloniaProperty.RegisterDirect<FurniIconImage, IImage?>(nameof(CurrentImage),
            x => x.CurrentImage);

    public static readonly DirectProperty<FurniIconImage, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<FurniIconImage, bool>(nameof(IconImageUrl),
            x => x.IsLoading);

    private IItemIcon? _icon;
    public IItemIcon? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }

    private string? _iconImageUrl;
    public string? IconImageUrl
    {
        get => _iconImageUrl;
        private set => SetAndRaise(IconImageUrlProperty, ref _iconImageUrl, value);
    }

    private AdvancedImage? _iconImage;

    public IImage? CurrentImage => _iconImage?.CurrentImage;
    public bool IsLoading => _iconImage?.IsLoading ?? false;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IconProperty)
        {
            if (Icon is { Revision: int revision, Identifier: string identifier })
            {
                identifier = identifier.Replace('*', '_');
                if (Icon is { Variant: string variant })
                    identifier += variant;

                IconImageUrl = UrlHelper.FurniIconUrl(identifier, revision);
            }
            else
            {
                IconImageUrl = null;
            }
        }

        base.OnPropertyChanged(change);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_iconImage is not null)
        {
            _iconImage.PropertyChanged -= OnImagePropertyChanged;
        }

        _iconImage = e.NameScope.Find<AdvancedImage>("PART_IconImage");
        if (_iconImage is not null)
        {
            _iconImage.PropertyChanged += OnImagePropertyChanged;
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        if (_iconImage is not null)
        {
            _iconImage.PropertyChanged -= OnImagePropertyChanged;
            _iconImage = null;
        }
    }

    private void OnImagePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == AdvancedImage.CurrentImageProperty)
        {
            RaisePropertyChanged<IImage?>(CurrentImageProperty, e.OldValue as IImage, e.NewValue as IImage);
        }
        else if (e.Property == AdvancedImage.IsLoadingProperty)
        {
            RaisePropertyChanged<bool>(IsLoadingProperty,
                (e.OldValue as bool?).GetValueOrDefault(),
                (e.NewValue as bool?).GetValueOrDefault());
        }
    }
}