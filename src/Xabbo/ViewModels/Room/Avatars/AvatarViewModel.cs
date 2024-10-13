using System.Reactive.Linq;
using ReactiveUI;

using Xabbo.Core;

namespace Xabbo.ViewModels;

public class AvatarViewModel : ViewModelBase
{
    public IAvatar Avatar { get; }

    public AvatarType Type => Avatar.Type;
    public int Index => Avatar.Index;
    public long Id => Avatar.Id;
    public string Name => Avatar.Name;
    public string Motto => Avatar.Motto;

    [Reactive] public bool IsStaff { get; set; }
    [Reactive] public bool IsOwner { get; set; }
    [Reactive] public RightsLevel RightsLevel { get; set; }

    [Reactive] public bool IsIdle { get; set; }
    [Reactive] public bool IsTrading { get; set; }

    public bool IsOrigins { get; set; }
    [Reactive] public string? ModernFigure { get; set; }

    public bool IsUser => Avatar.Type == AvatarType.User;

    readonly ObservableAsPropertyHelper<AvatarViewModelGroup> _group;
    public AvatarViewModelGroup Group => _group.Value;

    public AvatarViewModel(IAvatar avatar)
    {
        Avatar = avatar;
        IsIdle = avatar.IsIdle;
        IsTrading = avatar.CurrentUpdate?.IsTrading ?? false;

        _group = this
            .WhenAnyValue(
                x => x.IsStaff,
                x => x.IsOwner,
                x => x.RightsLevel,
                (isStaff, isOwner, controlLevel) => Avatar.Type switch
                {
                    AvatarType.Pet => AvatarViewModelGroup.Pets,
                    AvatarType.PrivateBot or AvatarType.PublicBot => AvatarViewModelGroup.Bots,
                    _ when isStaff => AvatarViewModelGroup.Staff,
                    _ when isOwner => AvatarViewModelGroup.RoomOwner,
                    _ => controlLevel switch
                    {
                        >= RightsLevel.Owner => AvatarViewModelGroup.RoomOwner,
                        >= RightsLevel.GroupAdmin => AvatarViewModelGroup.GroupAdmins,
                        >= RightsLevel.Standard => AvatarViewModelGroup.RightsHolders,
                        _ => AvatarViewModelGroup.Users
                    }
                }
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.Group);
    }
}
