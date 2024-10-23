using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public sealed class AreaFloorItemPlacement(Area area) : IFloorItemPlacement
{
    public Area Area { get; } = area;

    private HashSet<Point> _invalidPoints = [];

    public Task InitializeAsync(IRoom room)
    {
        _invalidPoints = [];
        return Task.CompletedTask;
    }

    public Task<Point?> FindLocationAsync(IRoom room, Point size, CancellationToken cancellationToken)
    {
        Point p = room
            .FindPlaceablePoints(Area, size, false)
            .FirstOrDefault(Point.MinValue);

        if (p == Point.MinValue)
            return Task.FromResult<Point?>(null);

        return Task.FromResult<Point?>(p);
    }

    public void ReportPlacementFailure(Point location)
    {
        _invalidPoints.Add(location);
    }

    public void Dispose() { }
}