using Xabbo.Components;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public sealed class ManualFloorItemPlacement(
    IExtension ext,
    XabbotComponent xabbot
)
    : IFloorItemPlacement, IDisposable
{
    private readonly IExtension _ext = ext;
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

    public async Task<Point?> FindLocationAsync(IRoom room, Point size, CancellationToken cancellationToken)
    {
        await _pointSemaphore.WaitAsync(cancellationToken);

        lock (_points)
        {
            if (_points.TryDequeue(out Point point))
                return point;
            else
                return null;
        }
    }

    public void ReportPlacementFailure(Point location)
    {
        _xabbot.ShowMessage($"Failed to place item at {location}");
    }

    public void Dispose()
    {
        _intercept?.Dispose();
    }
}