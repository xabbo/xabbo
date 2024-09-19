using Microsoft.Extensions.Configuration;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Services.Abstractions;

namespace Xabbo.Components;

public class NotificationComponent : Component
{
    private readonly IApplicationManager _app;
    private readonly ProfileManager _profileManager;
    private readonly FriendManager _friendManager;
    private readonly RoomManager _roomManager;

    private bool _flashOnPrivateMessage;
    public bool FlashOnPrivateMessage
    {
        get => _flashOnPrivateMessage;
        set => Set(ref _flashOnPrivateMessage, value);
    }

    private bool _flashOnWhisper;
    public bool FlashOnWhisper
    {
        get => _flashOnWhisper;
        set => Set(ref _flashOnWhisper, value);
    }

    private bool _flashOnUserChat;
    public bool FlashOnUserChat
    {
        get => _flashOnUserChat;
        set => Set(ref _flashOnUserChat, value);
    }

    private bool _flashOnFriendChat;
    public bool FlashOnFriendChat
    {
        get => _flashOnFriendChat;
        set => Set(ref _flashOnFriendChat, value);
    }

    private bool _flashOnUserEntered;
    public bool FlashOnUserEntered
    {
        get => _flashOnUserEntered;
        set => Set(ref _flashOnUserEntered, value);
    }

    private bool _flashOnFriendEntered;
    public bool FlashOnFriendEntered
    {
        get => _flashOnFriendEntered;
        set => Set(ref _flashOnFriendEntered, value);
    }

    public NotificationComponent(
        IExtension extension,
        IConfiguration config,
        IApplicationManager app,
        ProfileManager profileManager,
        FriendManager friendManager,
        RoomManager roomManager)
        : base(extension)
    {
        FlashOnPrivateMessage = config.GetValue("FlashWindow:OnPrivateMessage", true);
        FlashOnWhisper = config.GetValue("FlashWindow:OnWhisper", true);
        FlashOnUserChat = config.GetValue("FlashWindow:OnUserChat", false);
        FlashOnFriendChat = config.GetValue("FlashWindow:OnFriendChat", false);
        FlashOnUserEntered = config.GetValue("FlashWindow:OnUserEntered", false);
        FlashOnFriendEntered = config.GetValue("FlashWindow:OnFriendEntered", true);

        _app = app;
        _profileManager = profileManager;
        _friendManager = friendManager;
        _roomManager = roomManager;

        _friendManager.MessageReceived += OnReceivedPrivateMessage;

        _roomManager.AvatarsAdded += OnAvatarsAdded;
        _roomManager.AvatarChat += OnAvatarChat;
    }

    private void OnReceivedPrivateMessage(FriendMessageEventArgs e)
    {
        if (FlashOnPrivateMessage)
        {
            _app.FlashWindow();
        }
    }

    private void OnAvatarChat(AvatarChatEventArgs e)
    {
        if (e.Avatar is not User user) return;

        if (FlashOnUserChat ||
            (FlashOnWhisper && e.ChatType == ChatType.Whisper) ||
            (FlashOnFriendChat && _friendManager.IsFriend(user.Id)))
        {
            _app.FlashWindow();
        }
    }

    private void OnAvatarsAdded(AvatarsEventArgs e)
    {
        IEnumerable<User> users = e.Avatars.OfType<User>()
            .Where(u => u.Id != _profileManager.UserData?.Id);

        if ((FlashOnUserEntered && users.Any(u => u.Id != _profileManager.UserData?.Id)) ||
            (FlashOnFriendEntered && users.Any(u => _friendManager.IsFriend(u.Id))))
        {
            _app.FlashWindow();
        }
    }
}
