using Xabbo;
using Xabbo.Messages;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Ext.Components;

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

    public FurniActionsComponent(IExtension extension,
        IGameDataManager gameDataManager, RoomManager roomManager,
        XabbotComponent xabbot)
        : base(extension)
    {
        _gameDataManager = gameDataManager;
        _roomManager = roomManager;
        _xabbot = xabbot;

        _gameDataManager.Loaded += () => CanShowInfo = true;
    }

    [Intercept]
    private void HandleUseStuff(Intercept e, UseFloorItemMsg use)
    {
        IRoom? room = _roomManager.Room;
        if (room is null) return;

        if (PreventUse) e.Block();

        IFloorItem? item = room.GetFloorItem(use.Id);
        if (item == null) return;

        if (UseToHide)
        {
            e.Block();
            _roomManager.HideFurni(ItemType.Floor, use.Id);
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
                    // TODO FloorItemDataUpdateMsg
                    // Ext.Send(In.StuffDataUpdate, linkedItem.Id.ToString(), 0, "2");
                    // await Task.Delay(500);
                    // Extension.Send(In.StuffDataUpdate, linkedItem.Id.ToString(), 0, "0");
                }
                else
                {
                    // Extension.Send(In.StuffDataUpdate, linkedItem.Id, 0, "2");
                    // await Task.Delay(500);
                    // Extension.Send(In.StuffDataUpdate, linkedItem.Id, 0, "0");
                }
            }
        }
    }

    [Intercept]
    private void HandleUseWallItem(Intercept e, UseWallItemMsg use)
    {
        IRoom? room = _roomManager.Room;
        if (room is null) return;

        if (PreventUse) e.Block();

        IWallItem? item = room.GetWallItem(use.Id);
        if (item is null) return;

        if (UseToHide)
        {
            _roomManager.HideFurni(ItemType.Wall, use.Id);
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
