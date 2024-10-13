using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Extension;
using Xabbo.Messages.Flash;

namespace Xabbo.Controllers;

[Intercept]
public partial class RoomRightsController : ControllerBase
{
    private readonly RoomManager _roomManager;
    private readonly ILogger _logger;
    private int _requiredCount;

    public RoomRightsController(
        IExtension extension,
        ILoggerFactory loggerFactory,
        RoomManager roomManager
    ) : base(extension)
    {
        _logger = loggerFactory.CreateLogger<RoomRightsController>();
        _roomManager = roomManager;
        _roomManager.Entered += OnEnteredRoom;
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        if (_requiredCount > 0 && _roomManager.RightsLevel is RightsLevel.None)
        {
            GiveRights();
        }
    }

    public IDisposable RequireRights()
    {
        int count = Interlocked.Increment(ref _requiredCount);
        _logger.LogDebug("Rights required. (count: {Count})", count);

        if (count > 0) GiveRights();

        return Disposable.Create(UnrequireRights);
    }

    private void UnrequireRights()
    {
        int count = Interlocked.Decrement(ref _requiredCount);
        _logger.LogDebug("Rights unrequired. (count: {Count})", count);

        if (count <= 0) ResetRights();
    }

    private void GiveRights()
    {
        if (_roomManager.EnsureInRoom(out var room) &&
            _roomManager.RightsLevel is RightsLevel.None)
        {
            _logger.LogDebug("Giving client-side rights.");
            if (Session.Is(ClientType.Modern))
                Send(In.YouAreController, room.Id, 4);
            else
                Send(In.YouAreOwner);
        }
    }

    private void ResetRights()
    {
        if (_roomManager.EnsureInRoom(out var room))
        {
            if (_roomManager.RightsLevel is RightsLevel.None)
            {
                _logger.LogDebug("Removing client-side rights.");
                Send(In.YouAreNotController, room.Id);
            }
        }
    }
}