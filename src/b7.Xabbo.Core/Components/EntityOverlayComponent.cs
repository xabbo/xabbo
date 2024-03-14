using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Messages;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using ReactiveUI;

namespace b7.Xabbo.Components;

public class EntityOverlayComponent : Component
{
    private const int GHOST_INDEX = int.MinValue;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private bool _isInjected;

    private bool _isAvailable;
    public bool IsAvailable
    {
        get => _isAvailable;
        set => Set(ref _isAvailable, value);
    }

    public EntityOverlayComponent(IExtension extension,
        IConfiguration config,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _profileManager = profileManager;
        _roomManager = roomManager;
        _roomManager.Entered += OnEnteredRoom;
        _roomManager.EntityDataUpdated += OnEntityDataUpdated;
        _roomManager.Left += OnLeftRoom;

        IsActive = config.GetValue("EntityOverlay:Active", false);

        Task initialization = Task.Run(InitializeAsync);

        this.ObservableForProperty(x => x.IsActive)
            .Subscribe(x => OnIsActiveChanged(x.Value));
    }

    private void OnIsActiveChanged(bool isActive)
    {
        if (isActive)
            InjectGhostUser();
        else
            RemoveGhostUser();
    }

    private bool TryGetSelf([NotNullWhen(true)] out IRoomUser? self)
    {
        UserData? userData = _profileManager.UserData;
        IRoom? room = _roomManager.Room;

        if (userData is null || room is null)
        {
            self = null;
            return false;
        }
        else
        {
            return room.TryGetUserById(userData.Id, out self);
        }
    }

    private void OnEntityDataUpdated(object? sender, EntityDataUpdatedEventArgs e)
    {
        if (_isInjected &&
            e.Entity.Id == _profileManager.UserData?.Id &&
            e.Entity is IRoomUser self)
        {
            Extension.Send(In.UpdateAvatar,
                GHOST_INDEX,
                self.Figure,
                self.Gender.ToShortString(),
                self.Motto,
                self.AchievementScore
            );
        }
    }

    private async Task InitializeAsync()
    {
        await _profileManager.GetUserDataAsync();

        IsAvailable = true;
    }

    private void InjectGhostUser()
    {
        if (!TryGetSelf(out IRoomUser? self)) return;

        RoomUser ghostUser = new RoomUser(self.Id, GHOST_INDEX)
        {
            Name = self.Name,
            Figure = self.Figure,
            Gender = self.Gender,
            AchievementScore = self.AchievementScore,
            Location = self.Location + (32, 32, 32)
        };

        if (!_isInjected)
        {
            Extension.Send(In.UsersInRoom, 1, ghostUser);
            Extension.Send(In.RoomAvatarEffect, ghostUser.Index, 13, 0);
        }

        if (self.CurrentUpdate is not null)
        {
            Extension.Send(In.Status, 1, new EntityStatusUpdate(self.CurrentUpdate)
            {
                Index = ghostUser.Index,
                Location = self.CurrentUpdate.Location + (64, 64, 64)
            });
        }

        _isInjected = true;
    }

    private void RemoveGhostUser()
    {
        if (!_isInjected) return;

        Extension.Send(In.Status, 1, new EntityStatusUpdate()
        {
            Index = GHOST_INDEX,
            Location = (0, 0, -1000)
        });
    }

    private void OnEnteredRoom(object? sender, RoomEventArgs e)
    {
        if (IsAvailable && IsActive)
        {
            InjectGhostUser();
        }
    }

    private void OnLeftRoom(object? sender, EventArgs e)
    {
        _isInjected = false;
    }

    [InterceptIn(nameof(Incoming.Status))]
    protected void HandleStatus(InterceptArgs e)
    {
        if (!IsActive || !_isInjected) return;

        if (!TryGetSelf(out IRoomUser? self)) return;

        EntityStatusUpdate? selfUpdate = EntityStatusUpdate
            .ParseMany(e.Packet)
            .FirstOrDefault(x => x.Index == self.Index);

        if (selfUpdate is null) return;

        EntityStatusUpdate overlayUpdate = new EntityStatusUpdate(selfUpdate)
        {
            Index = GHOST_INDEX,
            Location = selfUpdate.Location + (32, 32, 32),
            MovingTo = selfUpdate.MovingTo + (32, 32, 32)
        };

        e.Packet.Position = 0;
        short n = e.Packet.ReadLegacyShort();

        e.Packet.Position = 0;
        e.Packet.WriteLegacyShort((short)(n + 1));
        e.Packet.Position = e.Packet.Length;
        e.Packet.Write(overlayUpdate);
    }
}
