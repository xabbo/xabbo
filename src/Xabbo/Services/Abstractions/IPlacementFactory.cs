using Xabbo.Core;

namespace Xabbo.Services.Abstractions;

public interface IPlacementFactory
{
    IFloorItemPlacement CreateAreaFloorPlacement(Area area);
    IFloorItemPlacement CreateManualFloorPlacement();

    IWallItemPlacement CreateRandomWallPlacement(Area area);
    IWallItemPlacement CreateManualWallPlacement();
}