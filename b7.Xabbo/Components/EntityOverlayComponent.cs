using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class EntityOverlayComponent : Component
    {
        private const int GHOST_INDEX = int.MinValue;

        private readonly ProfileManager _profileManager;
        private readonly RoomManager _roomManager;

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set => Set(ref _isAvailable, value);
        }

        public EntityOverlayComponent(IInterceptor interceptor,
            ProfileManager profileManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            _profileManager = profileManager;
            _roomManager = roomManager;
            _roomManager.Entered += OnRoomEntered;

            Task initialization = Task.Run(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            await _profileManager.GetUserDataAsync();

            IsAvailable = true;
        }

        private void InjectGhostUser()
        {
            UserData? userData = _profileManager.UserData;
            IRoom? room = _roomManager.Room;

            if (userData is null || room is null) return;

            if (!room.TryGetUserById(userData.Id, out IRoomUser? self))
                return;

            RoomUser ghostUser = new RoomUser(self.Id, GHOST_INDEX)
            {
                Name = self.Name,
                Figure = self.Figure,
                Gender = self.Gender,
                AchievementScore = self.AchievementScore,
                Location = self.Location.Add(32, 32, 32)
            };

            Send(In.UsersInRoom, 1, ghostUser);
            Send(In.RoomAvatarEffect, ghostUser.Index, 13, 0);

            if (self.CurrentUpdate is not null)
            {
                Send(In.Status, 1, new EntityStatusUpdate(self.CurrentUpdate)
                {
                    Index = ghostUser.Index,
                    Location = self.CurrentUpdate.Location.Add(32, 32, 32)
                });
            }
        }

        private void RemoveGhostUser()
        {
            UserData? userData = _profileManager.UserData;
            IRoom? room = _roomManager.Room;

            if (userData is null || room is null) return;

            if (!room.TryGetUserById(userData.Id, out IRoomUser? self))
                return;

            Send(In.UserLoggedOut, GHOST_INDEX.ToString());
        }

        private void OnRoomEntered(object? sender, RoomEventArgs e)
        {
            if (IsAvailable && IsActive)
            {
                InjectGhostUser();
            }
        }

        [InterceptIn(nameof(Incoming.Status))]
        protected void HandleStatus(InterceptArgs e)
        {
            UserData? userData = _profileManager.UserData;
            IRoom? room = _roomManager.Room;

            if (userData is null || room is null) return;
            if (!room.TryGetUserById(userData.Id, out IRoomUser? self)) return;

            EntityStatusUpdate? selfUpdate = EntityStatusUpdate
                .ParseMany(e.Packet)
                .FirstOrDefault(x => x.Index == self.Index);

            if (selfUpdate is null) return;

            EntityStatusUpdate overlayUpdate = new EntityStatusUpdate(selfUpdate)
            {
                Index = GHOST_INDEX,
                Location = selfUpdate.Location.Add(32, 32, 32),
                MovingTo = selfUpdate.MovingTo?.Add(32, 32, 32)
            };

            e.Packet.Position = 0;
            short n = e.Packet.ReadLegacyShort();

            e.Packet.Position = 0;
            e.Packet.WriteLegacyShort((short)(n + 1));
            e.Packet.Position = e.Packet.Length;
            e.Packet.Write(overlayUpdate);
        }

        public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            if (propertyName == nameof(IsActive))
            {
                if (IsActive)
                {
                    InjectGhostUser();
                }
                else
                {
                    RemoveGhostUser();
                }
            }
        }
    }
}
