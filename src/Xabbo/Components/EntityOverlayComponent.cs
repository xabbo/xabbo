using System.Diagnostics.CodeAnalysis;
using ReactiveUI;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;

namespace Xabbo.Components;

[Intercept]
public partial class AvatarOverlayComponent : Component
{
    private const int GhostIndex = -183;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private bool _isInjected;

    [Reactive] public bool Enabled { get; set; }

    public AvatarOverlayComponent(IExtension extension,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _profileManager = profileManager;

        _roomManager = roomManager;
        _roomManager.Entered += OnEnteredRoom;
        _roomManager.AvatarChanged += OnAvatarChanged;
        _roomManager.Left += OnLeftRoom;

        Task initialization = Task.Run(InitializeAsync);

        this.ObservableForProperty(x => x.Enabled)
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

    private void OnAvatarChanged(AvatarChangedEventArgs e)
    {
        if (_isInjected &&
            e.Avatar.Id == _profileManager.UserData?.Id &&
            e.Avatar is IUser self)
        {
            Ext.Send(In.UserChange,
                GhostIndex,
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

        User ghostUser = new User(self.Id, GhostIndex)
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
            Ext.Send(new AvatarStatusMsg {
                new AvatarStatus(self.CurrentUpdate)
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

        Ext.Send(new AvatarStatusMsg {
            new AvatarStatus
            {
                Index = GhostIndex,
                Location = (0, 0, -1000)
            }
        });
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        if (IsAvailable && Enabled)
        {
            InjectGhostUser();
        }
    }

    private void OnLeftRoom()
    {
        _isInjected = false;
    }

    [Intercept]
    protected void HandleStatus(Intercept e, AvatarStatusMsg updates)
    {
        if (!Enabled || !_isInjected) return;

        if (!TryGetSelf(out IUser? self)) return;

        AvatarStatus? selfUpdate = updates.FirstOrDefault(x => x.Index == self.Index);

        if (selfUpdate is null) return;

        AvatarStatus overlayUpdate = new AvatarStatus(selfUpdate)
        {
            Index = GhostIndex,
            Location = selfUpdate.Location + (32, 32, 32),
            MovingTo = selfUpdate.MovingTo + (32, 32, 32)
        };

        // Append the overlay update
        e.Packet.ModifyAt<Length>(0, n => (Length)(n + 1));
        e.Packet.WriteAt(e.Packet.Length, overlayUpdate);
    }
}
