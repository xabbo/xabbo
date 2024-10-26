using Microsoft.Extensions.Logging;
using Xabbo.Configuration;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Tasks;
using Xabbo.Exceptions;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Controllers;

[Intercept]
public partial class FurniPlacementController(
    ILoggerFactory loggerFactory,
    IConfigProvider<AppConfig> config,
    IExtension extension,
    RoomManager roomManager
)
    : ControllerBase(extension)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<FurniPlacementController>();
    private readonly IConfigProvider<AppConfig> _config = config;
    private readonly RoomManager _roomManager = roomManager;
    private readonly SemaphoreSlim _executionSemaphore = new(1, 1);

    public enum State { None, InitFloorPlacement, InitWallPlacement, PlacingFloorItems, PlacingWallItems }
    public enum RetryMode { None }
    public enum ErrorHandling { Abort, Retry, Skip }

    [Reactive] public State Status { get; set; } = State.None;
    [Reactive] public int Progress { get; set; }
    [Reactive] public int MaxProgress { get; set; }

    public async Task PlaceItemsAsync(IEnumerable<IInventoryItem> enumerable,
        IFloorItemPlacement floorItemPlacement,
        IWallItemPlacement wallItemPlacement,
        ErrorHandling errorHandling = ErrorHandling.Abort,
        CancellationToken cancellationToken = default)
    {
        if (!_executionSemaphore.Wait(0, CancellationToken.None))
            throw new Exception("Furni placer is already running.");

        try
        {
            if (!_roomManager.EnsureInRoom(out var room))
                throw new Exception("Room state is unavailable.");

            var floorItems = enumerable
                .Where(x => x.Type is ItemType.Floor)
                .ToArray();

            var wallItems = enumerable
                .Where(x => x.Type is ItemType.Wall)
                .ToArray();

            Progress = 1;
            MaxProgress = floorItems.Length + wallItems.Length;

            _logger.LogDebug("Placing {Count} items.", MaxProgress);

            if (floorItems.Length != 0)
            {
                using (floorItemPlacement)
                {
                    Status = State.InitFloorPlacement;
                    await floorItemPlacement.InitializeAsync(room);

                    Status = State.PlacingFloorItems;

                    int placementFailures = 0;
                    for (int i = 0; i < floorItems.Length; i++)
                    {
                        var item = floorItems[i];

                        if (!item.TryGetSize(out var size))
                        {
                            _logger.LogWarning("Failed to get size for {Item}.", item);
                            continue;
                        }

                        if (placementFailures >= 3)
                            throw new Exception("Failed to place an item too many times.");

                        Progress = i+1;

                        _logger.LogTrace("Placing item {Current}/{Max}.", Progress, MaxProgress);

                        if (!await PlaceFloorItemAsync(room, floorItemPlacement, item, size, errorHandling, cancellationToken))
                        {
                            i--;
                            placementFailures++;
                        }
                        else
                        {
                            placementFailures = 0;
                        }
                    }
                }
            }

            if (wallItems.Length != 0)
            {
                using (wallItemPlacement)
                {
                    Status = State.InitWallPlacement;
                    await wallItemPlacement.InitializeAsync(room);

                    Status = State.PlacingWallItems;

                    int placementFailures = 0;
                    for (int i = 0; i < wallItems.Length; i++)
                    {
                        if (placementFailures >= 3)
                            throw new Exception("Failed to place an item 3 times.");

                        Progress = floorItems.Length + i + 1;

                        _logger.LogTrace("Placing item {Current}/{Max}.", Progress, MaxProgress);

                        var item = wallItems[i];
                        if (!await PlaceWallItemAsync(room, wallItemPlacement, item, errorHandling, cancellationToken))
                        {
                            i--;
                            placementFailures++;
                        }
                        else
                        {
                            placementFailures = 0;
                        }
                    }
                }
            }
        }
        finally
        {
            _executionSemaphore.Release();

            Progress = 0;
            MaxProgress = 0;
        }
    }

    private async Task<bool> PlaceFloorItemAsync(
        IRoom room, IFloorItemPlacement floorItemPlacement, IInventoryItem item, Point size,
        ErrorHandling errorHandling, CancellationToken cancellationToken
    )
    {
        int direction = 0;
        if (item.TryGetInfo(out var info))
            direction = info.DefaultDirection;

        if (direction == 2 || direction == 6)
            size = size.Flip();

        Point location = await floorItemPlacement.FindLocationAsync(room, size, cancellationToken)
            ?? throw new PlacementNotFoundException();

        _logger.LogTrace("Placing {Item} at {Point}.", item, location);
        var interval = Task.Delay(_config.Value.Timing.GetTiming(Session).FurniPlaceInterval, cancellationToken);
        var result = await new PlaceFloorItemTask(Ext, item.ItemId, location, 0).ExecuteAsync(3000, cancellationToken);
        if (result is PlaceFloorItemTask.Result.Error)
        {
            _logger.LogWarning("Failed to place {Item}.", item);
            if (errorHandling is ErrorHandling.Abort)
                throw new Exception("Failed to place a floor item.");
            floorItemPlacement.ReportPlacementFailure(location);
        }
        await interval;

        return result is PlaceFloorItemTask.Result.Success;
    }

    private async Task<bool> PlaceWallItemAsync(
        IRoom room, IWallItemPlacement wallItemPlacement, IInventoryItem item,
        ErrorHandling errorHandling, CancellationToken cancellationToken)
    {
        WallLocation location = await wallItemPlacement.FindLocationAsync(room, cancellationToken)
            ?? throw new PlacementNotFoundException();

        _logger.LogTrace("Placing {Item} at {Location}.", item, location);
        var interval = Task.Delay(_config.Value.Timing.GetTiming(Session).FurniPlaceInterval, cancellationToken);
        var result = await new PlaceWallItemTask(Ext, item.ItemId, location).ExecuteAsync(3000, cancellationToken);

        if (result == PlaceWallItemTask.Result.Error)
        {
            _logger.LogWarning("Failed to place {Item}.", item);
            if (errorHandling is ErrorHandling.Abort)
                throw new Exception("Failed to place a wall item.");
            wallItemPlacement.ReportPlacementFailure(location);
        }
        await interval;

        return result is PlaceWallItemTask.Result.Success;
    }
}