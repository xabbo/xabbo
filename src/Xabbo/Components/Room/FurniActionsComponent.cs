using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Controllers;
using Xabbo.Utility;
using Xabbo.Services.Abstractions;

namespace Xabbo.Components;

[Intercept]
public partial class FurniActionsComponent : Component
{
    private readonly ILogger _logger;
    private readonly IHabboApi _api;
    private readonly IGameDataManager _gameDataManager;
    private readonly RoomRightsController _rightsController;
    private readonly RoomManager _roomManager;
    private readonly XabbotComponent _xabbot;

    private IDisposable? _requiredRights;

    private FurniData? FurniData => _gameDataManager.Furni;
    private ExternalTexts? Texts => _gameDataManager.Texts;

    [Reactive] public bool PreventUse { get; set; }
    [Reactive] public bool PickToHide { get; set; }
    [Reactive] public bool PickToFindLink { get; set; }
    [Reactive] public bool CanShowInfo { get; set; }
    [Reactive] public bool PickToShowInfo { get; set; }
    [Reactive] public bool PickToFetchMarketplaceStats { get; set; }

    public FurniActionsComponent(IExtension extension,
        ILoggerFactory loggerFactory,
        IHabboApi api,
        IGameDataManager gameDataManager,
        RoomRightsController rightsController,
        RoomManager roomManager,
        XabbotComponent xabbot)
        : base(extension)
    {
        _logger = loggerFactory.CreateLogger<FurniActionsComponent>();
        _api = api;
        _gameDataManager = gameDataManager;
        _rightsController = rightsController;
        _roomManager = roomManager;
        _xabbot = xabbot;

        this.WhenAnyValue(
                x => x.PickToHide,
                x => x.PickToShowInfo,
                x => x.PickToFindLink,
                x => x.PickToFetchMarketplaceStats,
                (a1, a2, a3, a4) => a1 || a2 || a3 || a4
            )
            .DistinctUntilChanged()
            .Subscribe(requiresRights => {
                _logger.LogDebug("needs rights: {Test}", requiresRights);
                if (requiresRights)
                {
                    _requiredRights ??= _rightsController.RequireRights();
                }
                else
                {
                    _requiredRights?.Dispose();
                    _requiredRights = null;
                }
            });

        _gameDataManager.Loaded += () => CanShowInfo = true;
    }

    [Intercept]
    private void HandleUseFloorItem(Intercept<UseFloorItemMsg> e)
    {
        if (PreventUse) e.Block();
    }

    [Intercept]
    private void HandleUseWallItem(Intercept<UseWallItemMsg> e)
    {
        if (PreventUse) e.Block();
    }

    [Intercept]
    private void HandlePick(Intercept e, PickupFurniMsg pick)
    {
        if (PickToHide || PickToShowInfo || PickToFindLink || PickToFetchMarketplaceStats)
            e.Block();

        IRoom? room = _roomManager.Room;
        if (room is null)
        {
            _logger.LogWarning("User is not in a room.");
            return;
        }

        IFurni? furni = room.GetFurni(pick.Type, pick.Id);
        if (furni is null)
        {
            _logger.LogWarning("Failed to find {Type} item #{Id}.", pick.Type, pick.Id);
            return;
        }

        if (PickToHide)
            _roomManager.HideFurni(furni);

        if (PickToShowInfo && CanShowInfo && FurniData is not null)
        {
            if (furni.TryGetInfo(out var info))
            {
                if (!furni.TryGetName(out string? name))
                    name = info.Identifier;

                if (furni is IFloorItem floor)
                {
                    _xabbot.ShowMessage($"{name} [{info.Identifier}] (id:{furni.Id}) {floor.Location} {floor.Direction}", floor.Location);
                }
                else if (furni is IWallItem wallItem)
                {
                    _xabbot.ShowMessage($"{name} [{info.Identifier}] (id:{furni.Id}) {wallItem.Location}", wallItem.Location.Wall);
                }
            }
        }

        if (PickToFindLink && furni is IFloorItem floorItem)
        {
            IFloorItem? linkedItem = room.GetFloorItem(floorItem.Extra);
            if (linkedItem is not null)
            {
                Task.Run(async () => {
                    Ext.Send(new FloorItemDataUpdatedMsg(linkedItem.Id, new LegacyData { Value = "1" }));
                    Ext.SlideFurni(linkedItem, to: linkedItem.Location + (0, 0, 1), duration: 500);
                    await Task.Delay(1000);
                    Ext.Send(new FloorItemDataUpdatedMsg(linkedItem.Id, new LegacyData { Value = "2" }));
                    await Task.Delay(1000);
                    Ext.SlideFurni(linkedItem, from: linkedItem.Location + (0, 0, 1), duration: 500);
                    Ext.Send(new FloorItemDataUpdatedMsg(linkedItem.Id, new LegacyData { Value = "0" }));
                });
            }
        }

        if (PickToFetchMarketplaceStats)
        {
            if (furni.TryGetInfo(out var furniInfo))
            {
                Task.Run(async () => {
                    try
                    {
                        var stats = await _api.FetchMarketplaceItemStats(Ext.Session.Hotel, furni.Type, furniInfo.Identifier);
                        int totalSold = stats.History.Sum(x => x.TotalSoldItems);
                        _xabbot.ShowMessage($"{furniInfo.Name} [{furniInfo.Identifier}]: average {stats.AveragePrice}c / "
                            + $"{totalSold} sold in the last {stats.HistoryLimitInDays} days");
                    }
                    catch (Exception ex)
                    {
                        _xabbot.ShowMessage($"Failed to fetch marketplace stats: {ex.Message}");
                    }
                });
            }
        }
    }
}
