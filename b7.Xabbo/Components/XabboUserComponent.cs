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
    public class XabboUserComponent : Component, IHostedService
    {
        private readonly RoomManager _roomManager;

        public XabboUserComponent(IInterceptor interceptor,
            RoomManager roomManager)
            : base(interceptor)
        {
            _roomManager = roomManager;
            _roomManager.Entered += OnEnteredRoom;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private void OnEnteredRoom(object? sender, RoomEventArgs e)
        {
            Bot bot = new(EntityType.PublicBot, 0xb7, -0xb7)
            {
                Name = "xabbo",
                Motto = "enhanced habbo",
                Location = new Tile(0, 0, -1000),
                Gender = Gender.Male,
                Figure = "hr-100.hd-185-14.ch-805-71.lg-281-75.sh-305-80.ea-1406.cc-260-80"
            };

            Send(In.RoomUsers, (LegacyShort)1, bot);
        }
    }
}
