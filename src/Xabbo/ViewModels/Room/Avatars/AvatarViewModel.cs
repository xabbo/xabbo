using ReactiveUI;

using Xabbo.Core;

namespace Xabbo.ViewModels;

public class AvatarViewModel : ViewModelBase
{
    private readonly IAvatar _avatar;

    public AvatarType Type => _avatar.Type;
    public int Index => _avatar.Index;
    public long Id => _avatar.Id;
    public string Name => _avatar.Name;
    public string Motto => _avatar.Motto;
    public string AvatarImageUrl => $"https://www.habbo.com/habbo-imaging/avatarimage?figure=${_avatar.Figure}";

    [Reactive] public bool IsStaff { get; set; }
    [Reactive] public bool IsOwner { get; set; }
    [Reactive] public RightsLevel RightsLevel { get; set; }

    [Reactive] public bool IsIdle { get; set; }
    [Reactive] public bool IsTrading { get; set; }

    public bool IsUser => _avatar.Type == AvatarType.User;

    readonly ObservableAsPropertyHelper<AvatarViewModelGroup> _group;
    public AvatarViewModelGroup Group => _group.Value;

    public AvatarViewModel(IAvatar avatar)
    {
        _avatar = avatar;
        IsIdle = avatar.IsIdle;
        IsTrading = avatar.CurrentUpdate?.IsTrading ?? false;

        _group = this
            .WhenAnyValue(
                x => x.IsStaff,
                x => x.IsOwner,
                x => x.RightsLevel,
                (isStaff, isOwner, controlLevel) => _avatar.Type switch
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
            .ToProperty(this, x => x.Group);
    }
}
