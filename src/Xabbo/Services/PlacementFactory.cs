using Xabbo.Components;
using Xabbo.Core;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public sealed class PlacementFactory(
    IExtension extension,
    XabbotComponent xabbot
)
    : IPlacementFactory
{
    private readonly IExtension _extension = extension;
    private readonly XabbotComponent _xabbot = xabbot;

    public IFloorItemPlacement CreateAreaFloorPlacement(Area area) => new AreaFloorItemPlacement(area);
    public IFloorItemPlacement CreateManualFloorPlacement() => new ManualFloorItemPlacement(_extension, _xabbot);
    public IWallItemPlacement CreateRandomWallPlacement(Area area) => new RandomWallItemPlacement(area);
    public IWallItemPlacement CreateManualWallPlacement() => new ManualWallItemPlacement(_extension, _xabbot);
}