using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Xabbo.Messages;
using Xabbo.Interceptor;

using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core;

namespace b7.Xabbo.Components
{
    public class XabbotComponent : Component, IHostedService
    {
        private readonly ProfileManager _profileManager;
        private readonly RoomManager _roomManager;

        public long UserId { get; private set; } = 0xb7;
        public int UserIndex { get; private set; } = -0xb7;

        public XabbotComponent(IInterceptor interceptor,
            ProfileManager profileManager, RoomManager roomManager)
            : base(interceptor)
        {
            _profileManager = profileManager;
            _roomManager = roomManager;
            _roomManager.Entered += OnEnteredRoom;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private void OnEnteredRoom(object? sender, RoomEventArgs e)
        {
            Bot bot = new(EntityType.PublicBot, UserId, UserIndex)
            {
                Name = "xabbo",
                Motto = "enhanced habbo",
                Location = new Tile(0, 0, -100),
                Gender = Gender.Male,
                Figure = "hr-100.hd-185-14.ch-805-71.lg-281-75.sh-305-80.ea-1406.cc-260-80"
            };

            Send(In.UsersInRoom, (LegacyShort)1, bot);
        }

        public void ShowMessage(string message)
        {
            (int X, int Y) location = (0, 0);

            IUserData? userData = _profileManager.UserData;
            IRoom? room = _roomManager.Room;

            if (userData is not null && room is not null &&
                room.TryGetUserById(userData.Id, out IRoomUser? user))
            {
                location = user.Location;
            }

            ShowMessage(message, location);
        }

        public void ShowMessage(string message, (int X, int Y) location)
        {
            Send(In.Status, 1, new EntityStatusUpdate
            {
                Index = UserIndex,
                Location = new Tile(location.X, location.Y, -100),
                Direction = 4,
                HeadDirection = 4
            });
            Send(In.Whisper, UserIndex, message, 0, 30, 0, 0);
        }
    }
}
