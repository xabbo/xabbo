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
    public enum Operation { None, Pickup, Eject }

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

    private async Task PickupOrEjectFurniAsync(IEnumerable<IFurni> furni, bool eject)
    {
        if (!_workingSemaphore.Wait(0))
            throw new Exception("An operation is currently in progress.");

        try
        {
            _cts = new CancellationTokenSource();

            var toPick = furni.ToArray();

            CurrentProgress = 0;
            TotalProgress = toPick.Length;
            CurrentOperation = eject ? Operation.Eject : Operation.Pickup;

            await _operationManager.RunAsync($"{(eject ? "eject" : "pickup")} furni", async (ct) => {
                for (int i = 0; i < toPick.Length; i++)
                {
                    if (i > 0)
                        await Task.Delay(ClampInterval(GetTiming().FurniPickupInterval), ct);
                    Send(new PickupFurniMsg(toPick[i]));
                    CurrentProgress = i+1;
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