using Xabbo.Configuration;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Controllers;

[Intercept]
public partial class RoomFurniController : ControllerBase
{
    public enum Operation { None, Pickup, Eject, Toggle, Rotate, Move }

    private readonly IConfigProvider<AppConfig> _config;
    private readonly IOperationManager _operationManager;
    private readonly SemaphoreSlim _workingSemaphore = new(1, 1);
    private CancellationTokenSource? _cts;

    private static int ClampInterval(int interval) => interval > 0 ? interval : 0;
    private TimingConfigBase GetTiming() => Session.Is(ClientType.Origins)
        ? _config.Value.Timing.Origins
        : _config.Value.Timing.Modern;

    [Reactive] public Operation CurrentOperation { get; private set; }
    [Reactive] public int CurrentProgress { get; private set; }
    [Reactive] public int TotalProgress { get; private set; }

    public RoomFurniController(
        IExtension extension,
        IConfigProvider<AppConfig> config,
        IOperationManager operationManager,
        RoomManager roomManager
    )
        : base(extension)
    {
        _config = config;
        _operationManager = operationManager;

        roomManager.Left += OnLeftRoom;
    }

    public Task PickupFurniAsync(IEnumerable<IFurni> furni) => PickupOrEjectFurniAsync(furni, false);
    public Task EjectFurniAsync(IEnumerable<IFurni> furni) => PickupOrEjectFurniAsync(furni, true);
    public void CancelCurrentOperation() => _cts?.Cancel();

    private void OnLeftRoom() => CancelCurrentOperation();

    public Task ToggleFurniAsync(IEnumerable<IFurni> furni) => ProcessFurniAsync(
        Operation.Toggle,
        furni,
        x => x.FurniToggleInterval,
        f => Send(f is IFloorItem ? new UseFloorItemMsg(f.Id) : new UseWallItemMsg(f.Id))
    );

    public Task RotateFurniAsync(IEnumerable<IFurni> furni, Directions direction) => ProcessFurniAsync(
        Operation.Rotate,
        furni,
        x => x.FurniRotateInterval,
        f => Send(new MoveFloorItemMsg(f.Id, ((IFloorItem)f).Location, (int)direction)),
        filter: f => f is IFloorItem floorItem && floorItem.Direction != (int)direction
    );

    public Task MoveFurniAsync(IEnumerable<IFurni> furni) => ProcessFurniAsync(
        Operation.Move,
        furni,
        x => 0,
        async (f, ct) => {
            var walk = await ReceiveAsync<WalkMsg>(timeout: -1, block: true, cancellationToken: ct);
            Send(new MoveFloorItemMsg(f.Id, walk.Point, ((IFloorItem)f).Direction));
        },
        filter: f => f is IFloorItem
    );

    private Task PickupOrEjectFurniAsync(IEnumerable<IFurni> furni, bool eject) => ProcessFurniAsync(
        eject ? Operation.Eject : Operation.Pickup,
        furni,
        x => x.FurniPickupInterval,
        f => Send(new PickupFurniMsg(f))
    );

    private Task ProcessFurniAsync(Operation operation,
        IEnumerable<IFurni> furni,
        Func<TimingConfigBase, int> timingSelector,
        Action<IFurni> action,
        Func<IFurni, bool>? filter = null)
    {
        return ProcessFurniAsync(
            operation,
            furni,
            timingSelector,
            (f, _) => { action(f); return Task.CompletedTask; },
            filter
        );
    }

    private async Task ProcessFurniAsync(Operation operation,
        IEnumerable<IFurni> furni,
        Func<TimingConfigBase, int> timingSelector,
        Func<IFurni, CancellationToken, Task> action,
        Func<IFurni, bool>? filter = null)
    {
        if (!_workingSemaphore.Wait(0))
            throw new Exception("An operation is currently in progress.");

        try
        {
            _cts = new CancellationTokenSource();

            if (filter is not null)
                furni = furni.Where(filter);

            var toProcess = furni.ToArray();

            CurrentProgress = 0;
            TotalProgress = toProcess.Length;
            CurrentOperation = operation;

            await _operationManager.RunAsync($"{operation} furni", async (ct) => {
                for (int i = 0; i < toProcess.Length; i++)
                {
                    CurrentProgress = i+1;
                    if (i > 0)
                        await Task.Delay(ClampInterval(timingSelector(GetTiming())), ct);
                    await action(toProcess[i], ct);
                }
            }, _cts.Token);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            CurrentOperation = Operation.None;
            _workingSemaphore.Release();
        }
    }
}