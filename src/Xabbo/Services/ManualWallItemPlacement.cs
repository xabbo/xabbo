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

    private IDisposable? _intercept;

    private readonly Queue<Point> _points = [];
    private readonly SemaphoreSlim _pointSemaphore = new(0);

    public Task InitializeAsync(IRoom room)
    {
        _intercept?.Dispose();
        _intercept = _ext.Intercept<WalkMsg>(HandleWalkMsg);
        return Task.CompletedTask;
    }

    private void HandleWalkMsg(Intercept e, WalkMsg walk)
    {
        e.Block();

        lock (_points)
        {
            _points.Enqueue(walk.Point);
        }

        _pointSemaphore.Release();
    }

    public async Task<WallLocation?> FindLocationAsync(IRoom room, CancellationToken cancellationToken)
    {
        int scale = room.FloorPlan.Scale;

        while (true)
        {
            await _pointSemaphore.WaitAsync(cancellationToken);

            lock (_points)
            {
                if (!_points.TryDequeue(out Point wallTile))
                    return null;

                if (!room.FloorPlan.IsWalkable(wallTile - (0, 1)))
                    return new WallLocation(wallTile - (0, 1), (scale/2, scale), 'r');
                else if (!room.FloorPlan.IsWalkable(wallTile - (1, 0)))
                    return new WallLocation(wallTile - (1, 0), (scale/2, scale), 'l');
                else
                    _xabbot.ShowMessage("That is not a valid wall tile. Click a tile in front of a wall.");
            }
        }
    }

    public void ReportPlacementFailure(WallLocation location) { }

    public void Dispose() { }
}