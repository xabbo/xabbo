using Xabbo.Core;
using Xabbo.Core.Game;

namespace Xabbo.Services.Abstractions;

public interface IWallItemPlacement : IDisposable
{
    /// <summary>
    /// Initializes the placement locator.
    /// </summary>
    Task InitializeAsync(IRoom room);

    /// <summary>
    /// Attempts to locate a point to where a wall item can probably be placed.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A point where a wall item can probably be placed, or null if a location could not be found.</returns>
    Task<WallLocation?> FindLocationAsync(IRoom room, CancellationToken cancellationToken);

    /// <summary>
    /// Reports a failure to place an item at the specified location.
    /// </summary>
    /// <param name="location">The location where an item failed to place.</param>
    void ReportPlacementFailure(WallLocation location);
}