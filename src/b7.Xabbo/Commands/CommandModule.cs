using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Messages.Dispatcher;
using Xabbo.Connection;
using Xabbo.Extension;

namespace b7.Xabbo.Commands;

public abstract class CommandModule : ConnectionBase, IMessageHandler
{
    public bool IsAvailable { get; set; }
    public CommandManager Commands { get; private set; } = null!;

    protected IExtension Extension => Commands.Extension;
    protected IMessageDispatcher Dispatcher => Extension.Dispatcher;
    protected IMessageManager Messages => Extension.Messages;
    protected Incoming In => Messages.In;
    protected Outgoing Out => Messages.Out;

    public override void Send(IReadOnlyPacket packet) => Extension.Send(packet);
    public override ValueTask SendAsync(IReadOnlyPacket packet) => Extension.SendAsync(packet);
    public override Task<IPacket> ReceiveAsync(HeaderSet headers,
        int timeout = -1, bool block = false, CancellationToken cancellationToken = default)
        => Extension.ReceiveAsync(headers, timeout, block, cancellationToken);
    public override Task<IPacket> ReceiveAsync(HeaderSet headers, Func<IReadOnlyPacket, bool> shouldCapture,
        int timeout = -1, bool block = false, CancellationToken cancellationToken = default)
        => Extension.ReceiveAsync(headers, shouldCapture, timeout, block, cancellationToken);

    public override CancellationToken DisconnectToken => Extension.DisconnectToken;
    public override ClientType Client => Extension.Client;
    public override string ClientIdentifier => Extension.ClientIdentifier;
    public override bool IsConnected => IsConnected;


    public CommandModule() { }

    public void Initialize(CommandManager manager)
    {
        Commands = manager;

        OnInitialize();
    }

    protected virtual void OnInitialize() { IsAvailable = true; }

    protected void ShowMessage(string message) => Commands.ShowMessage(message);

    protected Task<IPacket> ReceiveAsync(Header header, int timeout = -1, bool block = false,
        CancellationToken cancellationToken = default)
    {
        return Extension.ReceiveAsync(header, timeout, block, cancellationToken);
    }

    protected Task<IPacket> ReceiveAsync(ITuple tuple, int timeout = -1, bool block = false,
        CancellationToken cancellationToken = default)
    {
        return Extension.ReceiveAsync(HeaderSet.FromTuple(tuple), timeout, block, cancellationToken);
    }
}
