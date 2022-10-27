using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Microsoft.Extensions.Configuration;

using Xabbo.Extension;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core;

using b7.Xabbo.Util;

namespace b7.Xabbo.Components;

public class FlashWindowComponent : Component
{
    private readonly ProfileManager _profileManager;
    private readonly FriendManager _friendManager;
    private readonly RoomManager _roomManager;

    private Process? _currentProcess;

    private bool _flashOnPrivateMessage;
    public bool FlashOnPrivateMessage
    {
        get => _flashOnPrivateMessage;
        set => SetProperty(ref _flashOnPrivateMessage, value);
    }

    private bool _flashOnWhisper;
    public bool FlashOnWhisper
    {
        get => _flashOnWhisper;
        set => SetProperty(ref _flashOnWhisper, value);
    }

    private bool _flashOnUserChat;
    public bool FlashOnUserChat
    {
        get => _flashOnUserChat;
        set => SetProperty(ref _flashOnUserChat, value);
    }


    private bool _flashOnFriendChat;
    public bool FlashOnFriendChat
    {
        get => _flashOnFriendChat;
        set => SetProperty(ref _flashOnFriendChat, value);
    }

    private bool _flashOnUserEntered;
    public bool FlashOnUserEntered
    {
        get => _flashOnUserEntered;
        set => SetProperty(ref _flashOnUserEntered, value);
    }

    private bool _flashOnFriendEntered;
    public bool FlashOnFriendEntered
    {
        get => _flashOnFriendEntered;
        set => SetProperty(ref _flashOnFriendEntered, value);
    }

    public FlashWindowComponent(IExtension extension,
        IConfiguration config,
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

        _profileManager = profileManager;
        _friendManager = friendManager;
        _roomManager = roomManager;

        _friendManager.MessageReceived += OnReceivedPrivateMessage;

        _roomManager.EntitiesAdded += OnEntitiesAdded;
        _roomManager.EntityChat += OnEntityChat;
    }

    protected override void OnDisconnected(object? sender, EventArgs e)
    {
        base.OnDisconnected(sender, e);

        _currentProcess = null;
    }

    private Process? FindProcess()
    {
        Process? process = null;

        UserData? userData = _profileManager.UserData;
        if (userData is not null)
        {
            process = Process.GetProcessesByName("habbo").FirstOrDefault(p =>
                p.MainWindowTitle.StartsWith("Habbo") &&
                p.MainWindowTitle.EndsWith(userData.Name)
            );
        }

        return process;
    }

    private void FlashWindow()
    {
        _currentProcess ??= FindProcess();
        if (_currentProcess is null) return;

        WindowUtil.FlashWindow(_currentProcess.MainWindowHandle);
    }

    private void OnReceivedPrivateMessage(object? sender, FriendMessageEventArgs e)
    {
        if (FlashOnPrivateMessage)
        {
            FlashWindow();
        }
    }

    private void OnEntityChat(object? sender, EntityChatEventArgs e)
    {
        if (e.Entity is not IRoomUser user) return;

        if (FlashOnUserChat ||
            (FlashOnWhisper && e.ChatType == ChatType.Whisper) ||
            (FlashOnFriendChat && _friendManager.IsFriend(user.Id)))
        {
            FlashWindow();
        }
    }

    private void OnEntitiesAdded(object? sender, EntitiesEventArgs e)
    {
        IEnumerable<IRoomUser> users = e.Entities.OfType<IRoomUser>()
            .Where(u => u.Id != _profileManager.UserData?.Id);

        if ((FlashOnUserEntered && users.Any(u => u.Id != _profileManager.UserData?.Id)) ||
            (FlashOnFriendEntered && users.Any(u => _friendManager.IsFriend(u.Id))))
        {
            FlashWindow();
        }
    }
}
