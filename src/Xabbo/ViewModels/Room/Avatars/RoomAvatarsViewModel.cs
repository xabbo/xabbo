using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Web;
using DynamicData;
using DynamicData.Kernel;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

using Xabbo.Configuration;
using Xabbo.Controllers;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Exceptions;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;
using Xabbo.Utility;

namespace Xabbo.ViewModels;

public class RoomAvatarsViewModel : ViewModelBase
{
    private readonly IExtension _ext;
    private readonly IConfigProvider<AppConfig> _config;
    private readonly IUiContext _uiContext;
    private readonly IDialogService _dialog;
    private readonly IClipboardService _clipboard;
    private readonly ILauncherService _launcher;
    private readonly WardrobePageViewModel _wardrobe;
    private readonly IOperationManager _operations;
    private readonly IFigureConverterService _figureConverter;
    private readonly RoomModerationController _moderation;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;
    private readonly TradeManager _tradeManager;

    private readonly SourceCache<AvatarViewModel, int> _avatarCache = new(x => x.Index);

    private readonly ReadOnlyObservableCollection<AvatarViewModel> _avatars;
    public ReadOnlyObservableCollection<AvatarViewModel> Avatars => _avatars;

    [Reactive] public string FilterText { get; set; } = "";

    [Reactive] public IList<AvatarViewModel>? ContextSelection { get; set; }

    private readonly ObservableAsPropertyHelper<IEnumerable<IUser>> _selectedUsers;
    public IEnumerable<IUser> SelectedUsers => _selectedUsers.Value;

    public event Action? RefreshList;

    public RoomUsersConfig Config => _config.Value.Room.Users;

    public ReactiveCommand<Unit, Unit> FindAvatarCmd { get; }
    public ReactiveCommand<Unit, Unit> CopyAvatarToWardrobeCmd { get; }
    public ReactiveCommand<Unit, Unit> TradeUserCmd { get; }
    public ReactiveCommand<string, Unit> CopyAvatarFieldCmd { get; }
    public ReactiveCommand<string, Task> OpenUserProfileCmd { get; }

    public ReactiveCommand<string, Task> MuteUsersCmd { get; }
    public ReactiveCommand<Unit, Task> KickUsersCmd { get; }
    public ReactiveCommand<BanDuration, Task> BanUsersCmd { get; }
    public ReactiveCommand<Unit, Task> BounceUsersCmd { get; }

    public ReactiveCommand<Unit, Unit> CancelCmd { get; }

    private readonly ObservableAsPropertyHelper<bool> _isBusy;
    public bool IsBusy => _isBusy.Value;

    private readonly ObservableAsPropertyHelper<string> _statusText;
    public string StatusText => _statusText.Value;

    public RoomAvatarsViewModel(
        IExtension ext,
        IConfigProvider<AppConfig> config,
        IUiContext uiContext,
        IDialogService dialog,
        IClipboardService clipboard,
        ILauncherService launcher,
        IOperationManager operations,
        IFigureConverterService figureConverter,
        RoomModerationController moderation,
        WardrobePageViewModel wardrobe,
        ProfileManager profileManager,
        RoomManager roomManager,
        TradeManager tradeManager)
    {
        _ext = ext;
        _config = config;
        _uiContext = uiContext;
        _dialog = dialog;
        _clipboard = clipboard;
        _launcher = launcher;
        _operations = operations;
        _figureConverter = figureConverter;
        _wardrobe = wardrobe;
        _moderation = moderation;
        _profileManager = profileManager;
        _roomManager = roomManager;
        _tradeManager = tradeManager;

        _selectedUsers = this
            .WhenAnyValue(
                x => x.ContextSelection,
                x => x?.Select(vm => vm.Avatar).OfType<IUser>() ?? []
            )
            .ToProperty(this, x => x.SelectedUsers);

        _isBusy = moderation
            .WhenAnyValue(
                x => x.CurrentOperation,
                x => x.TotalProgress,
                (op, totalProgress) =>
                    op is not RoomModerationController.ModerationType.None
                    and not RoomModerationController.ModerationType.Unban &&
                    totalProgress > 1
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.IsBusy);

        _statusText = moderation
            .WhenAnyValue(
                x => x.CurrentOperation,
                x => x.CurrentProgress,
                x => x.TotalProgress,
                (op, current, total) => $"{
                    op switch
                    {
                        RoomModerationController.ModerationType.Mute => "Muting",
                        RoomModerationController.ModerationType.Unmute => "Unmuting",
                        RoomModerationController.ModerationType.Kick => "Kicking",
                        RoomModerationController.ModerationType.Ban => "Banning",
                        RoomModerationController.ModerationType.Bounce => "Bouncing",
                        _ => "Moderating"
                    }
                } users...\n{current} / {total}"
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.StatusText);

        _avatarCache
            .Connect()
            .Filter(
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.FilterText),
                    _config.WhenAnyValue(
                        x => x.Value.Room.Users.ShowPets,
                        x => x.Value.Room.Users.ShowBots,
                        (showPets, showBots) => (showPets, showBots)
                    ),
                    (filterText, config) => (filterText, config)
                )
                .Select(x => CreateFilter(x.filterText, x.config.showPets, x.config.showBots))
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _avatars, AvatarViewModelGroupComparer.Default)
            .Subscribe();

        var hasSingleContextAvatar = this
            .WhenAnyValue(x => x.ContextSelection)
            .Select(x => x is [ { } ])
            .ObserveOn(RxApp.MainThreadScheduler);

        var hasSingleContextUser = this
            .WhenAnyValue(x => x.ContextSelection)
            .Select(x => x is [ { Avatar.Type: AvatarType.User } ])
            .ObserveOn(RxApp.MainThreadScheduler);

        var hasAnyContextUser = this
            .WhenAnyValue(x => x.ContextSelection)
            .Select(x => x?.Any(avatar => avatar.Type is AvatarType.User) == true)
            .ObserveOn(RxApp.MainThreadScheduler);

        var hasAnyNonSelfUser = this
            .WhenAnyValue(x => x.ContextSelection)
            .Select(x => x?.Any(avatar =>
                avatar.Type is AvatarType.User &&
                avatar.Id != _profileManager.UserData?.Id &&
                avatar.Name != _profileManager.UserData?.Name
            ) == true)
            .ObserveOn(RxApp.MainThreadScheduler);

        var hasSingleNonSelfUser = this
            .WhenAnyValue(x => x.ContextSelection)
            .Select(x =>
                x is [ { Avatar.Type: AvatarType.User } user ] &&
                user.Id != _profileManager.UserData?.Id &&
                user.Name != _profileManager.UserData?.Name
            )
            .ObserveOn(RxApp.MainThreadScheduler);

        FindAvatarCmd = ReactiveCommand.Create(FindAvatar, hasSingleContextAvatar);
        CopyAvatarToWardrobeCmd = ReactiveCommand.Create(CopyAvatarsToWardrobe, hasAnyContextUser);
        TradeUserCmd = ReactiveCommand.Create(TradeUser,
            Observable.CombineLatest(
                hasSingleNonSelfUser,
                _tradeManager.WhenAnyValue(x => x.IsTrading),
                (hasUser, isTrading) => hasUser && !isTrading
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );
        CopyAvatarFieldCmd = ReactiveCommand.Create<string>(CopyAvatarField, hasSingleContextUser);
        OpenUserProfileCmd = ReactiveCommand.Create<string, Task>(OpenUserProfile, hasSingleContextUser);

        var selectedUsers = this.WhenAnyValue(
            x => x.ContextSelection,
            x => x?.Select(x => x.Avatar).OfType<IUser>() ?? []
        );

        MuteUsersCmd = ReactiveCommand.Create<string, Task>(
            MuteUsersAsync,
            Observable.CombineLatest(
                profileManager.WhenAnyValue(x => x.UserData),
                moderation.WhenAnyValue(
                    x => x.CurrentOperation,
                    x => x.CanMute,
                    x => x.RightsLevel,
                    (op, canMute, rightsLevel) => (
                        canModerate: op is RoomModerationController.ModerationType.None && canMute,
                        rightsLevel
                    )
                ),
                selectedUsers,
                (userData, mod, users) =>
                    mod.canModerate &&
                    users.Any(user =>
                        user.Id != userData?.Id &&
                        user.RightsLevel < mod.rightsLevel &&
                        !user.IsStaff
                    ) == true
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        KickUsersCmd = ReactiveCommand.Create<Task>(
            KickUsersAsync,
            Observable.CombineLatest(
                profileManager.WhenAnyValue(x => x.UserData),
                moderation.WhenAnyValue(
                    x => x.CurrentOperation,
                    x => x.CanKick,
                    x => x.RightsLevel,
                    (op, canKick, rightsLevel) => (
                        canModerate: op is RoomModerationController.ModerationType.None && canKick,
                        rightsLevel
                    )
                ),
                selectedUsers,
                (userData, mod, users) =>
                    mod.canModerate &&
                    users.Any(user =>
                        user.Id != userData?.Id &&
                        user.RightsLevel < mod.rightsLevel &&
                        !user.IsStaff
                    ) == true
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        BanUsersCmd = ReactiveCommand.Create<BanDuration, Task>(
            BanUsersAsync,
            Observable.CombineLatest(
                profileManager.WhenAnyValue(x => x.UserData),
                moderation.WhenAnyValue(
                    x => x.CurrentOperation,
                    x => x.CanBan,
                    x => x.RightsLevel,
                    (op, canBan, rightsLevel) => (
                        canModerate: op is RoomModerationController.ModerationType.None && canBan,
                        rightsLevel
                    )
                ),
                selectedUsers,
                (userData, mod, users) => {
                    return mod.canModerate &&
                    users.Any(user =>
                        user.Id != userData?.Id &&
                        user.RightsLevel < mod.rightsLevel &&
                        !user.IsStaff
                    ) == true;
                }
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        BounceUsersCmd = ReactiveCommand.Create<Task>(
            BounceUsersAsync,
            Observable.CombineLatest(
                profileManager.WhenAnyValue(x => x.UserData),
                moderation.WhenAnyValue(
                    x => x.CurrentOperation,
                    x => x.RightsLevel,
                    x => x.IsOwner,
                    (op, rightsLevel, isOwner) =>
                        op is RoomModerationController.ModerationType.None &&
                        (rightsLevel is RightsLevel.Owner || isOwner)
                ),
                selectedUsers,
                (userData, canModerate, users) =>
                    canModerate &&
                    users.Any(user =>
                        user.Id != userData?.Id &&
                        user.Name != userData?.Name &&
                        !user.IsStaff
                    ) == true
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        CancelCmd = ReactiveCommand.Create(
            () => { _operations.TryCancelOperation(out _); },
            this.WhenAnyValue(x => x.IsBusy).ObserveOn(RxApp.MainThreadScheduler)
        );

        _roomManager.Left += OnLeftRoom;
        _roomManager.AvatarsAdded += OnAvatarsAdded;
        _roomManager.AvatarRemoved += OnAvatarRemoved;
        _roomManager.AvatarIdle += OnAvatarIdle;
        _roomManager.AvatarsUpdated += OnAvatarsUpdated;

        _figureConverter.Available += OnFigureConverterAvailable;
    }

    private void UpdateOriginsFigure(AvatarViewModel vm)
    {
        if (vm.Type is AvatarType.User && vm.IsOrigins &&
            _figureConverter.TryConvertToModern(vm.Avatar.Figure, out Figure? figure))
        {
            vm.ModernFigure = figure.ToString();
        }
    }

    private void OnFigureConverterAvailable()
    {
        if (_ext.Session.Is(ClientType.Origins))
        {
            foreach (var (_, vm) in _avatarCache.KeyValues)
                UpdateOriginsFigure(vm);
        }
    }

    private void CopyAvatarsToWardrobe()
    {
        if (ContextSelection is { } selection)
        {
            foreach (var vm in selection)
            {
                if (vm.Avatar is User user)
                {
                    _wardrobe.AddFigure(user.Gender, user.Figure);
                }
            }
        }
    }

    private Task MuteUsersAsync(string minutesStr) => TryModerate(() => _moderation.MuteUsersAsync(SelectedUsers, int.Parse(minutesStr)));
    private Task KickUsersAsync() => TryModerate(() => _moderation.KickUsersAsync(SelectedUsers));
    private Task BanUsersAsync(BanDuration duration) => TryModerate(() => _moderation.BanUsersAsync(SelectedUsers, duration));
    private Task BounceUsersAsync() => TryModerate(() => _moderation.BounceUsersAsync(SelectedUsers));

    private async Task TryModerate(Func<Task> moderate)
    {
        try
        {
            await moderate();
        }
        catch (OperationInProgressException ex)
        {
            await _dialog.ShowAsync("Error", ex.Message);
        }
        catch (Exception ex)
        {
            await _dialog.ShowAsync("Error", ex.Message);
        }
    }

    private void FindAvatar()
    {
        if (ContextSelection is not [ var avatar ])
            return;

        _ext.Send(new AvatarWhisperMsg("(click here to find)", avatar.Index));
    }

    private void TradeUser()
    {
        if (ContextSelection is not [var avatar])
            return;

        _ext.Send(new TradeUserMsg(avatar.Index));
    }

    private void CopyAvatarField(string field)
    {
        if (ContextSelection is not [ var avatar ])
            return;

        switch (field)
        {
            case "id":
                _clipboard.SetText(avatar.Id.ToString());
                break;
            case "name":
                _clipboard.SetText(avatar.Name);
                break;
            case "motto":
                _clipboard.SetText(avatar.Motto);
                break;
            case "figure":
                _clipboard.SetText(avatar.Avatar.Figure);
                break;
        }
    }

    private async Task OpenUserProfile(string type)
    {
        if (ContextSelection is not [ { Avatar: IUser user } ])
            return;

        switch (type)
        {
            case "game":
                var profile = await _ext.RequestAsync(new GetProfileByNameMsg(user.Name), block: false);
                if (!profile.DisplayInClient)
                {
                    var result = await _dialog.ShowContentDialogAsync(
                        _dialog.CreateViewModel<MainViewModel>(),
                        new HanumanInstitute.MvvmDialogs.Avalonia.Fluent.ContentDialogSettings
                        {
                            Title = "Failed to open profile",
                            Content = $"{user.Name}'s profile is not visible.",
                            PrimaryButtonText = "OK",
                        }
                    );
                }
                break;
            case "web":
                _launcher.Launch($"https://{_ext.Session.Hotel.WebHost}/profile/{HttpUtility.UrlEncode(user.Name)}");
                break;
            case "habbowidgets":
                _launcher.Launch($"https://www.habbowidgets.com/habinfo/{_ext.Session.Hotel.Domain}/{HttpUtility.UrlEncode(user.Name)}");
                break;
        }
    }

    static Func<AvatarViewModel, bool> CreateFilter(string? filterText, bool showPets, bool showBots)
    {
        return (avatar) => {
            if (!showPets && avatar.Type == AvatarType.Pet)
                return false;
            if (!showBots && avatar.Type is AvatarType.PublicBot or AvatarType.PrivateBot)
                return false;
            if (!string.IsNullOrWhiteSpace(filterText) &&
                !avatar.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            return true;
        };
    }

    private void OnAvatarsUpdated(AvatarsEventArgs e)
    {
        _uiContext.Invoke(() => {
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
                _avatarCache.Refresh();
                RefreshList?.Invoke();
            }
        });
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
                var vm = new AvatarViewModel(avatar) { IsOrigins = _ext.Session.Is(ClientType.Origins) };
                if (avatar is User user)
                {
                    if (_roomManager.Room?.Data is { OwnerId: Id ownerId, OwnerName: string ownerName })
                    {
                        if (ownerId > 0)
                        {
                            if (user.Id == ownerId)
                                vm.IsOwner = true;
                        }
                        else if (!string.IsNullOrWhiteSpace(ownerName) && user.Name == ownerName)
                        {
                            vm.IsOwner = true;
                        }
                    }
                    vm.IsStaff = user.IsStaff;
                    if (_ext.Session.Is(ClientType.Modern))
                        vm.ModernFigure = avatar.Figure;
                    else
                        UpdateOriginsFigure(vm);
                }

                _avatarCache.AddOrUpdate(vm);
            }
        });
    }

    private void OnAvatarRemoved(AvatarEventArgs e)
    {
        _uiContext.Invoke(() => { _avatarCache.RemoveKey(e.Avatar.Index); });
    }

    private void OnAvatarIdle(AvatarIdleEventArgs e)
    {
        _avatarCache.Lookup(e.Avatar.Index).IfHasValue(vm =>
        {
            vm.IsIdle = e.Avatar.IsIdle;
        });
    }
}
