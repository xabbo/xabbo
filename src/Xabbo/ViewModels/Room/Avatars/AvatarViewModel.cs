using System;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using Avalonia.Media.Imaging;

using Xabbo.Core;
using Xabbo.Utility;

namespace Xabbo.ViewModels;

public class AvatarViewModel : ViewModelBase
{
    private readonly Lazy<Task<Bitmap?>> _avatarImageLoader;

    private readonly IAvatar _avatar;

    public AvatarType Type => _avatar.Type;
    public int Index => _avatar.Index;
    public long Id => _avatar.Id;
    public string Name => _avatar.Name;
    public string Motto => _avatar.Motto;
    public string AvatarImageUrl => $"https://www.habbo.com/habbo-imaging/avatarimage?figure=${_avatar.Figure}";
    public Task<Bitmap?> AvatarImage => _avatarImageLoader.Value;

    [Reactive] public bool IsIdle { get; set; }
    [Reactive] public bool IsTrading { get; set; }

    public bool IsUser => _avatar.Type == AvatarType.User;

    public AvatarViewModel(IAvatar avatar)
    {
        _avatar = avatar;

        IsIdle = avatar.IsIdle;
        IsTrading = avatar.CurrentUpdate?.IsTrading ?? false;

        _avatarImageLoader = new Lazy<Task<Bitmap?>>(() =>
        {
            if (avatar.Type == AvatarType.User)
            {
                return ImageHelper.LoadFromWeb(new Uri(AvatarImageUrl));
            }
            else
            {
                return Task.FromResult<Bitmap?>(null);
            }
        });
    }
}
