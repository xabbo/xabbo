using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Extension;
using Xabbo.Messages;
using Xabbo.Messages.Dispatcher;

namespace b7.Xabbo.Commands;

public abstract class CommandModule : IMessageHandler
{
    public bool IsAvailable { get; set; }
    public CommandManager Commands { get; private set; } = null!;

    protected IExtension Extension => Commands.Extension;
    protected IMessageDispatcher Dispatcher => Extension.Dispatcher;
    protected IMessageManager Messages => Extension.Messages;
    protected Incoming In => Messages.In;
    protected Outgoing Out => Messages.Out;

    protected void Send(Header header, params object[] values) => Extension.Send(header, values);
    protected void Send(IReadOnlyPacket packet) => Extension.Send(packet);

    protected ValueTask SendAsync(Header header, params object[] values) => Extension.SendAsync(header, values);
    protected ValueTask SendAsync(IReadOnlyPacket packet) => Extension.SendAsync(packet);

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
