using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Extensions.Options;

using Xabbo.Messages;
using Xabbo.Interceptor;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

using b7.Xabbo.Configuration;

namespace b7.Xabbo.Components
{
    public class ChatComponent : Component, INotifyPropertyChanged
    {
        private readonly RoomManager _roomManager;
        private readonly HashSet<string> _petCommands;

        private bool _mutePets = true;
        public bool MutePets
        {
            get => _mutePets;
            set => Set(ref _mutePets, value);
        }

        private bool _mutePetCommands = true;
        public bool MutePetCommands
        {
            get => _mutePetCommands;
            set => Set(ref _mutePetCommands, value);
        }

        private bool _muteBots;
        public bool MuteBots
        {
            get => _muteBots;
            set => Set(ref _muteBots, value);
        }

        private bool _muteWired;
        public bool MuteWired
        {
            get => _muteWired;
            set => Set(ref _muteWired, value);
        }

        private bool _muteRespects;
        public bool MuteRespects
        {
            get => _muteRespects;
            set => Set(ref _muteRespects, value);
        }

        private bool _muteScratches;
        public bool MuteScratches
        {
            get => _muteScratches;
            set => Set(ref _muteScratches, value);
        }

        public ChatComponent(IInterceptor interceptor,
            IOptions<GameOptions> gameOptions,
            RoomManager roomManager)
            : base(interceptor)
        {
            _roomManager = roomManager;
            _roomManager.EntityChat += OnEntityChat;

            _petCommands = gameOptions.Value.PetCommands;
        }

        private void OnEntityChat(object? sender, EntityChatEventArgs e)
        {
            if (MutePets && e.Entity.Type == EntityType.Pet) e.Block();
            if (MuteBots)
            {
                if (e.Entity.Type == EntityType.PublicBot ||
                    e.Entity.Type == EntityType.PrivateBot)
                    e.Block();
            }

            if (MutePetCommands)
            {
                int index = e.Message.IndexOf(' ');
                if (index > 0)
                {
                    string name = e.Message.Substring(0, index);
                    if (name == "bobba" || _roomManager.Room?.GetEntity<IPet>(name) is not null)
                    {
                        string command = e.Message[(index + 1)..].ToLower();
                        if (_petCommands.Contains(command)) e.Block();
                    }
                }
            }

            if (MuteWired && e.BubbleStyle == 34) e.Block();
        }

        [InterceptIn(nameof(Incoming.RespectNotification))]
        private void OnRoomUserRespect(InterceptArgs e)
        {
            if (MuteRespects) e.Block();
        }

        [InterceptIn(nameof(Incoming.PetRespectNotification))]
        private void OnRoomPetRespect(InterceptArgs e)
        {
            if (MuteScratches) e.Block();
        }
    }
}
