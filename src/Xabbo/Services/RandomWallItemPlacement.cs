using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public sealed class RandomWallItemPlacement(Area area) : IWallItemPlacement
{
    private readonly Area _area = area;
    private List<WallLocation> _wallTiles = [];

    public Task InitializeAsync(IRoom room)
    {
        _wallTiles = [];
        for (int y = _area.Min.Y; y <= _area.Max.Y; y++)
        {
            for (int x = _area.Min.X; x <= _area.Max.X; x++)
            {
                if (!room.FloorPlan.Area.Contains(new Area(x-1, y-1, x, y)))
                    continue;

                if (room.FloorPlan[x, y] < 0)
                    continue;

                if (room.FloorPlan[x-1, y] < 0)
                    _wallTiles.Add(new WallLocation((x-1, y), Point.Zero, 'l'));

                if (room.FloorPlan[x, y-1] < 0)
                    _wallTiles.Add(new WallLocation((x, y-1), Point.Zero, 'r'));
            }
        }

        if (_wallTiles.Count == 0)
            throw new Exception("No wall tiles found.");

        return Task.CompletedTask;
    }

    public Task<WallLocation?> FindLocationAsync(IRoom room, CancellationToken cancellationToken)
    {
        if (_wallTiles.Count == 0)
            throw new Exception("No wall tiles found.");

        WallLocation location = _wallTiles[Random.Shared.Next(0, _wallTiles.Count)];
        location += (
            Random.Shared.Next(room.FloorPlan.Scale / 2),
            room.FloorPlan.Scale + Random.Shared.Next(room.FloorPlan.Scale)
        );

        return Task.FromResult<WallLocation?>(location);
    }

    public void ReportPlacementFailure(WallLocation location) { }

    public void Dispose() { }
}