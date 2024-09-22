using Xabbo.Core;

namespace Xabbo.ViewModels;

public class AvatarViewModel(IAvatar avatar) : ViewModelBase
{
    private readonly IAvatar _avatar = avatar;

    public AvatarType Type => _avatar.Type;
    public int Index => _avatar.Index;
    public long Id => _avatar.Id;
    public string Name => _avatar.Name;
    public string Motto => _avatar.Motto;
    public string AvatarImageUrl => $"https://www.habbo.com/habbo-imaging/avatarimage?figure=${_avatar.Figure}";

    [Reactive] public bool IsIdle { get; set; } = avatar.IsIdle;
    [Reactive] public bool IsTrading { get; set; } = avatar.CurrentUpdate?.IsTrading ?? false;

    public bool IsUser => _avatar.Type == AvatarType.User;
}
