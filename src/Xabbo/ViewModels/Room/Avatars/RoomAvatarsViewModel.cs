using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Kernel;
using ReactiveUI;

using Xabbo.Configuration;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public class RoomAvatarsViewModel : ViewModelBase
{
    private readonly IConfigProvider<AppConfig> _config;
    private readonly IUiContext _uiContext;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<AvatarViewModel, int> _avatarCache = new(x => x.Index);

    private readonly ReadOnlyObservableCollection<AvatarViewModel> _avatars;
    public ReadOnlyObservableCollection<AvatarViewModel> Avatars => _avatars;

    [Reactive] public string FilterText { get; set; } = "";

    public event Action? RefreshList;

    public RoomUsersConfig Config => _config.Value.Room.Users;

    public RoomAvatarsViewModel(IConfigProvider<AppConfig> config, IUiContext uiContext, RoomManager roomManager)
    {
        _config = config;
        _uiContext = uiContext;
        _roomManager = roomManager;

        _avatarCache
            .Connect()
            .Filter(FilterAvatar)
            .Sort(AvatarViewModelGroupComparer.Default)
            .Bind(out _avatars)
            .Subscribe();

        _config
            .WhenAnyValue(
                x => x.Value.Room.Users.ShowPets,
                x => x.Value.Room.Users.ShowBots
            )
            .Subscribe(x => {
                _avatarCache.Refresh();
                RefreshList?.Invoke();
            });

        this.WhenAnyValue(x => x.FilterText).Subscribe(_ => _avatarCache.Refresh());

        _roomManager.Left += OnLeftRoom;
        _roomManager.AvatarsAdded += OnAvatarsAdded;
        _roomManager.AvatarRemoved += OnAvatarRemoved;
        _roomManager.AvatarIdle += OnAvatarIdle;
        _roomManager.AvatarsUpdated += OnAvatarsUpdated;
    }

    private bool FilterAvatar(AvatarViewModel avatar)
    {
        if (!Config.ShowPets && avatar.Type == AvatarType.Pet)
            return false;
        if (!Config.ShowBots && avatar.Type is AvatarType.PublicBot or AvatarType.PrivateBot)
            return false;
        if (!string.IsNullOrWhiteSpace(FilterText) && !avatar.Name.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase))
            return false;

        return true;
    }

    private void OnAvatarsUpdated(AvatarsEventArgs e)
    {
        bool shouldRefresh = false;

        foreach (var avatar in e.Avatars)
        {
            _avatarCache.Lookup(avatar.Index).IfHasValue(vm =>
            {
                var update = avatar.CurrentUpdate;
                if (update is null) return;
                vm.IsTrading = update.IsTrading;
                if (vm.RightsLevel != update.RightsLevel)
                {
                    vm.RightsLevel = update.RightsLevel;
                    shouldRefresh = true;
                }
            });
        }

        if (shouldRefresh)
        {
            _uiContext.InvokeAsync(() => {
                _avatarCache.Refresh();
                RefreshList?.Invoke();
            });
        }
    }

    private void OnLeftRoom()
    {
        _uiContext.Invoke(_avatarCache.Clear);
    }

    private void OnAvatarsAdded(AvatarsEventArgs e)
    {
        _uiContext.Invoke(() => {
            foreach (var avatar in e.Avatars)
            {
                var vm = new AvatarViewModel(avatar);
                if (avatar.Id == _roomManager?.CurrentRoomId)
                    vm.IsOwner = true;
                if (avatar is User user && user.IsStaff)
                    vm.IsStaff = true;

                _avatarCache.AddOrUpdate(vm);
            }
        });
    }

    private void OnAvatarRemoved(AvatarEventArgs e)
    {
        _uiContext.Invoke(() => _avatarCache.RemoveKey(e.Avatar.Index));
    }

    private void OnAvatarIdle(AvatarIdleEventArgs e)
    {
        _avatarCache.Lookup(e.Avatar.Index).IfHasValue(vm =>
        {
            vm.IsIdle = e.Avatar.IsIdle;
        });
    }
}
