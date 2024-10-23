using Xabbo.Components;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public sealed class ManualWallItemPlacement(
    IExtension extension,
    XabbotComponent xabbot
)
    : IWallItemPlacement
{
    private readonly IExtension _ext = extension;
    private readonly XabbotComponent _xabbot = xabbot;

    public Task InitializeAsync(IRoom room) => Task.CompletedTask;

    public async Task<WallLocation?> FindLocationAsync(IRoom room, CancellationToken cancellationToken)
    {
        while (true)
        {
            var wallTile = (await _ext.ReceiveAsync<WalkMsg>(timeout: -1, block: true, cancellationToken: cancellationToken)).Point;
            if (!room.FloorPlan.IsWalkable(wallTile - (1, 0)))
                return new WallLocation(wallTile - (1, 0), (0, 0), 'l');
            else if (!room.FloorPlan.IsWalkable(wallTile - (0, 1)))
                return new WallLocation();
            else
                _xabbot.ShowMessage("That is not a valid wall tile. Click a tile in front of a wall.");
        }
    }

    public void ReportPlacementFailure(WallLocation location) { }

    public void Dispose() { }
}