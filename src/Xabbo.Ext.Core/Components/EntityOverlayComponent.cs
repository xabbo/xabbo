using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;

using ReactiveUI;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;

namespace Xabbo.Ext.Components;

public class AvatarOverlayComponent : Component
{
    private const int GHOST_INDEX = int.MinValue;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private bool _isInjected;

    public AvatarOverlayComponent(IExtension extension,
        IConfiguration config,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _profileManager = profileManager;

        _roomManager = roomManager;
        _roomManager.Entered += OnEnteredRoom;
        _roomManager.AvatarDataUpdated += OnAvatarDataUpdated;
        _roomManager.Left += OnLeftRoom;

        IsActive = config.GetValue("AvatarOverlay:Active", false);

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

    private bool TryGetSelf([NotNullWhen(true)] out IUser? self)
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

    private void OnAvatarDataUpdated(AvatarDataUpdatedEventArgs e)
    {
        if (_isInjected &&
            e.Avatar.Id == _profileManager.UserData?.Id &&
            e.Avatar is IUser self)
        {
            Ext.Send(In.UserChange,
                GHOST_INDEX,
                self.Figure,
                self.Gender.ToClientString(),
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
        if (!TryGetSelf(out IUser? self)) return;

        User ghostUser = new User(self.Id, GHOST_INDEX)
        {
            Name = self.Name,
            Figure = self.Figure,
            Gender = self.Gender,
            AchievementScore = self.AchievementScore,
            Location = self.Location + (32, 32, 32)
        };

        if (!_isInjected)
        {
            Ext.Send(new AvatarsAddedMsg { ghostUser });
            Ext.Send(new AvatarEffectMsg(ghostUser.Index, 13));
        }

        if (self.CurrentUpdate is not null)
        {
            Ext.Send(new AvatarUpdatesMsg {
                new AvatarStatusUpdate(self.CurrentUpdate)
                {
                    Index = ghostUser.Index,
                    Location = self.CurrentUpdate.Location + (64, 64, 64)
                }
            });
        }

        _isInjected = true;
    }

    private void RemoveGhostUser()
    {
        if (!_isInjected) return;

        Ext.Send(new AvatarUpdatesMsg {
            new AvatarStatusUpdate
            {
                Index = GHOST_INDEX,
                Location = (0, 0, -1000)
            }
        });
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        if (IsAvailable && IsActive)
        {
            InjectGhostUser();
        }
    }

    private void OnLeftRoom()
    {
        _isInjected = false;
    }

    [Intercept]
    protected void HandleStatus(Intercept e, AvatarUpdatesMsg updates)
    {
        if (!IsActive || !_isInjected) return;

        if (!TryGetSelf(out IUser? self)) return;

        AvatarStatusUpdate? selfUpdate = updates.FirstOrDefault(x => x.Index == self.Index);

        if (selfUpdate is null) return;

        AvatarStatusUpdate overlayUpdate = new AvatarStatusUpdate(selfUpdate)
        {
            Index = GHOST_INDEX,
            Location = selfUpdate.Location + (32, 32, 32),
            MovingTo = selfUpdate.MovingTo + (32, 32, 32)
        };

        // Append the overlay update
        e.Packet.ModifyAt<Length>(0, n => n + 1);
        e.Packet.WriteAt(e.Packet.Length, overlayUpdate);
    }
}
