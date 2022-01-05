using System;
using System.Threading.Tasks;

using Xabbo;
using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;

namespace b7.Xabbo.Components
{
    public class FurniActionsComponent : Component
    {
        private readonly IGameDataManager _gameDataManager;
        private readonly RoomManager _roomManager;
        private readonly XabbotComponent _xabbot;

        private FurniData? FurniData => _gameDataManager.Furni;
        private ExternalTexts? Texts => _gameDataManager.Texts;

        private bool _preventUse;
        public bool PreventUse
        {
            get => _preventUse;
            set => Set(ref _preventUse, value);
        }

        private bool _useToHide;
        public bool UseToHide
        {
            get => _useToHide;
            set => Set(ref _useToHide, value);
        }

        private bool _useToFindLink;
        public bool UseToFindLink
        {
            get => _useToFindLink;
            set => Set(ref _useToFindLink, value);
        }

        private bool _canShowInfo;
        public bool CanShowInfo
        {
            get => _canShowInfo;
            set => Set(ref _canShowInfo, value);
        }

        private bool _useToShowInfo;
        public bool UseToShowInfo
        {
            get => _useToShowInfo;
            set => Set(ref _useToShowInfo, value);
        }

        public FurniActionsComponent(IInterceptor interceptor,
            IGameDataManager gameDataManager, RoomManager roomManager,
            XabbotComponent xabbot)
            : base(interceptor)
        {
            _gameDataManager = gameDataManager;
            _roomManager = roomManager;
            _xabbot = xabbot;

            _gameDataManager.Loaded += () => CanShowInfo = true;
        }

        [InterceptOut(nameof(Outgoing.UseStuff))]
        private async void HandleUseStuff(InterceptArgs e)
        {
            IRoom? room = _roomManager.Room;
            if (room is null) return;

            long id = e.Packet.ReadLegacyLong();

            if (PreventUse) e.Block();

            IFloorItem? item = room.GetFloorItem(id);
            if (item == null) return;

            if (UseToHide)
            {
                e.Block();
                _roomManager.HideFurni(ItemType.Floor, id);
            }

            if (UseToShowInfo && CanShowInfo && FurniData is not null)
            {
                FurniInfo info = FurniData.GetInfo(item);
                if (info != null)
                {
                    e.Block();

                    string name = info.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = info.Identifier;

                    _xabbot.ShowMessage($"{name} [{info.Identifier}] (id:{item.Id}) {item.Location} {item.Direction}", item.Location);
                }
            }

            if (UseToFindLink)
            {
                IFloorItem? linkedItem = room.GetFloorItem(item.Extra);
                if (linkedItem != null)
                {
                    if (Client == ClientType.Flash)
                    {
                        Send(In.StuffDataUpdate, linkedItem.Id.ToString(), 0, "2");
                        await Task.Delay(500);
                        Send(In.StuffDataUpdate, linkedItem.Id.ToString(), 0, "0");
                    }
                    else
                    {
                        Send(In.StuffDataUpdate, linkedItem.Id, 0, "2");
                        await Task.Delay(500);
                        Send(In.StuffDataUpdate, linkedItem.Id, 0, "0");
                    }
                }
            }
        }

        [InterceptOut(nameof(Outgoing.UseWallItem))]
        private async void HandleUseWallItem(InterceptArgs e)
        {
            IRoom? room = _roomManager.Room;
            if (room is null) return;

            long id = e.Packet.ReadLegacyLong();

            if (PreventUse) e.Block();

            IWallItem? item = room.GetWallItem(id);
            if (item is null) return;

            if (UseToHide)
            {
                _roomManager.HideFurni(ItemType.Wall, id);
                e.Block();
            }

            if (UseToShowInfo && CanShowInfo && FurniData is not null)
            {
                FurniInfo? info = FurniData.GetInfo(item);
                if (info is not null)
                {
                    e.Block();

                    string? name = info.Name;

                    if (info.Identifier == "poster")
                        Texts?.TryGetValue($"poster_{item.Data}_name", out name);

                    if (string.IsNullOrWhiteSpace(name))
                        name = info.Identifier;

                    _xabbot.ShowMessage($"{name} [{info.Identifier}] (id:{item.Id}) {item.Location}");
                }
            }
        }
    }
}
