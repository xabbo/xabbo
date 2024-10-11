using ReactiveUI;
using Xabbo.Configuration;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Exceptions;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Controllers;

[Intercept]
public partial class RoomModerationController : ControllerBase
{
    public enum ModerationType { None, Mute, Unmute, Kick, Ban, Unban, Bounce }
    delegate Task ModerateUserCallback(IUser user, Id roomId, CancellationToken cancellationToken);

    private readonly IConfigProvider<AppConfig> _config;
    private readonly IOperationManager _operationManager;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;
    private readonly SemaphoreSlim _workingSemaphore = new(1, 1);
    private CancellationTokenSource? _cts;

    private TimingConfigBase GetTiming() => Session.Is(ClientType.Origins)
        ? _config.Value.Timing.Origins
        : _config.Value.Timing.Modern;

    [Reactive] public ModerationType CurrentOperation { get; set; }
    [Reactive] public int CurrentProgress { get; set; }
    [Reactive] public int TotalProgress { get; set; }

    public RightsLevel RightsLevel => _roomManager.RightsLevel;
    public bool HasRights => _roomManager.HasRights;
    public bool CanMute => _roomManager.CanMute;
    public bool CanKick => _roomManager.CanKick;
    public bool CanBan => _roomManager.CanBan;
    public bool CanUnban => _roomManager.RightsLevel >= RightsLevel.GroupAdmin;
    public bool IsOwner => _roomManager.IsOwner;

    public RoomModerationController(
        IExtension extension,
        IConfigProvider<AppConfig> config,
        IOperationManager operationManager,
        ProfileManager profileManager,
        RoomManager roomManager
    )
        : base(extension)
    {
        _config = config;
        _operationManager = operationManager;
        _profileManager = profileManager;
        _roomManager = roomManager;

        _roomManager.Entered += (e) => RefreshPermissions();
        _roomManager.RoomDataUpdated += (e) => RefreshPermissions();
        _roomManager.RightsUpdated += RefreshPermissions;
        _roomManager.Left += OnLeftRoom;
    }

    public bool CanModerate(ModerationType type, IUser user) =>
        user.Name != _profileManager.UserData?.Name &&
        type switch
        {
            ModerationType.Mute or ModerationType.Unmute => CanMute && RightsLevel > user.RightsLevel && !user.IsStaff,
            ModerationType.Kick => CanKick && RightsLevel > user.RightsLevel && !user.IsStaff,
            ModerationType.Ban => CanBan && RightsLevel > user.RightsLevel && !user.IsStaff,
            ModerationType.Unban => RightsLevel >= RightsLevel.GroupAdmin,
            ModerationType.Bounce => IsOwner && !user.IsStaff,
            _ => false,
        };

    public Task MuteUsersAsync(IEnumerable<IUser> users, int minutes) => ModerateUsersAsync(
        minutes == 0 ? ModerationType.Unmute : ModerationType.Mute,
        (user, roomId, _) => MuteUserAsync(user, roomId, minutes),
        users
    );

    public Task UnmuteUsersAsync(IEnumerable<IUser> users) => MuteUsersAsync(users, 0);

    public Task KickUsersAsync(IEnumerable<IUser> users) => ModerateUsersAsync(ModerationType.Kick, (user, _, _) => KickUserAsync(user), users);

    public Task BanUsersAsync(IEnumerable<IUser> users, BanDuration duration)
        => ModerateUsersAsync(ModerationType.Ban, (user, room, _) => BanUserAsync(user, room, duration), users);

    public Task UnbanUsersAsync(IEnumerable<IdName> users) => ModerateUsersAsync(
        ModerationType.Unban,
        (user, room, _) => UnbanUserAsync(user, room),
        users.Select(x => new User(x.Id, -1) { Name = x.Name })
    );

    public Task BounceUsersAsync(IEnumerable<IUser> users)
        => ModerateUsersAsync(ModerationType.Bounce, BounceUserAsync, users);

    private void RefreshPermissions()
    {
        this.RaisePropertyChanged(nameof(RightsLevel));
        this.RaisePropertyChanged(nameof(HasRights));
        this.RaisePropertyChanged(nameof(CanMute));
        this.RaisePropertyChanged(nameof(CanKick));
        this.RaisePropertyChanged(nameof(CanBan));
        this.RaisePropertyChanged(nameof(CanUnban));
        this.RaisePropertyChanged(nameof(IsOwner));
    }

    private void OnLeftRoom()
    {
        _cts?.Cancel();

        RefreshPermissions();
    }

    private Task MuteUserAsync(IUser user, Id roomId, int minutes)
    {
        Send(new MuteUserMsg(user.Id, roomId, minutes));
        return Task.CompletedTask;
    }

    private Task KickUserAsync(IUser user)
    {
        Send(new KickUserMsg(user));
        return Task.CompletedTask;
    }

    private Task BanUserAsync(IUser user, Id roomId, BanDuration duration)
    {
        Send(new BanUserMsg(user.Id, user.Name, roomId, duration));
        return Task.CompletedTask;
    }

    private Task UnbanUserAsync(IUser user, Id roomId)
    {
        Send(new UnbanUserMsg(user.Id, roomId));
        return Task.CompletedTask;
    }

    private async Task BounceUserAsync(IUser user, Id roomId, CancellationToken cancellationToken)
    {
        Send(new BanUserMsg(user, roomId, BanDuration.Hour));
        await Task.Delay(_config.Value.Timing.Modern.BounceUnbanDelay, cancellationToken);
        Send(new UnbanUserMsg(user.Id, roomId));
    }

    private async Task ModerateUsersAsync(ModerationType type, ModerateUserCallback moderateUser, IEnumerable<IUser> users)
    {
        var toModerate = users.Where(user => CanModerate(type, user)).ToArray();
        if (toModerate.Length == 0)
            return;

        if (!_roomManager.EnsureInRoom(out var room))
            throw new Exception("The room state is not currently being tracked.");

        if (toModerate.Length == 1)
        {
            await moderateUser(toModerate[0], room.Id, CancellationToken.None);
        }
        else
        {
            if (!_workingSemaphore.Wait(0))
                throw new OperationInProgressException(CurrentOperation.ToString());

            try
            {
                _cts = new();

                await _operationManager.RunAsync(
                    type switch
                    {
                        ModerationType.Mute => "Mute users",
                        ModerationType.Unmute => "Unmute users",
                        ModerationType.Kick => "Kick users",
                        ModerationType.Ban => "Ban users",
                        ModerationType.Unban => "Unban users",
                        ModerationType.Bounce => "Bounce users",
                        _ => "Moderate users"
                    },
                    ct => DoModerateUsersAsync(type, moderateUser, room, toModerate, ct),
                    _cts.Token
                );
            }
            finally
            {
                _workingSemaphore.Release();
            }
        }
    }

    private async Task DoModerateUsersAsync(
        ModerationType operation,
        ModerateUserCallback moderateUser,
        IRoom? room, IUser[] users,
        CancellationToken cancellationToken)
    {
        CurrentOperation = operation;
        CurrentProgress = 1;
        TotalProgress = users.Length;

        try
        {
            for (int i = 0; i < users.Length; i++)
            {
                if (users[i].IsRemoved)
                    continue;
                if (i > 0)
                    await Task.Delay(GetTiming().ModerationInterval, cancellationToken);
                await moderateUser(users[i], room?.Id ?? -1, cancellationToken);
                CurrentProgress = i+1;
            }
        }
        finally
        {
            CurrentOperation = ModerationType.None;
        }
    }
}
