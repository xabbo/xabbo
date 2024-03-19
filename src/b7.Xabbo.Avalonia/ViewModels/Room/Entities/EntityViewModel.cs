using System;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;
using ReactiveUI.Fody.Helpers;

using Xabbo.Core;

using b7.Xabbo.Avalonia.Helpers;

namespace b7.Xabbo.Avalonia.ViewModels;

public class EntityViewModel : ViewModelBase
{
    private Lazy<Task<Bitmap?>> _avatarImageLoader;

    private readonly IEntity _entity;

    public EntityType Type => _entity.Type;
    public int Index => _entity.Index;
    public long Id => _entity.Id;
    public string Name => _entity.Name;
    public string Motto => _entity.Motto;
    public string AvatarImageUrl => $"https://www.habbo.com/habbo-imaging/avatarimage?figure=${_entity.Figure}";
    public Task<Bitmap?> AvatarImage => _avatarImageLoader.Value;

    [Reactive] public bool IsIdle { get; set; }
    [Reactive] public bool IsTrading { get; set; }

    public bool IsUser => _entity.Type == EntityType.User;

    public EntityViewModel(IEntity entity)
    {
        _entity = entity;

        IsIdle = entity.IsIdle;
        IsTrading = entity.CurrentUpdate?.IsTrading ?? false;

        _avatarImageLoader = new Lazy<Task<Bitmap?>>(() =>
        {
            if (entity.Type == EntityType.User)
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
