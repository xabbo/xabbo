using Xabbo.Core;
using Xabbo.Core.Game;

namespace Xabbo.Services.Abstractions;

public interface IFloorItemPlacement : IDisposable
{
    /// <summary>
    /// Initializes the placement locator.
    /// </summary>
    Task InitializeAsync(IRoom room);

    /// <summary>
    /// Attempts to locate a point to where a floor item can probably be placed.
    /// </summary>
    /// <param name="size">The size of the floor item.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A point where a floor item can probably be placed, or null if a location could not be found.</returns>
    Task<Point?> FindLocationAsync(IRoom room, Point size, CancellationToken cancellationToken);

    /// <summary>
    /// Reports a failure to place an item at the specified location.
    /// </summary>
    /// <param name="location">The location where an item failed to place.</param>
    void ReportPlacementFailure(Point location);
}
