using System.Collections;

namespace Xabbo.ViewModels;

public sealed class AvatarViewModelGroupComparer : IComparer<AvatarViewModel>, IComparer
{
    public static readonly AvatarViewModelGroupComparer Default = new();

    public int Compare(AvatarViewModel? x, AvatarViewModel? y)
    {
        if (x is null || y is null) return 0;
        return ((int)x.Group) - ((int)y.Group);
    }

    int IComparer.Compare(object? x, object? y)
        => Compare(x as AvatarViewModel, y as AvatarViewModel);
}