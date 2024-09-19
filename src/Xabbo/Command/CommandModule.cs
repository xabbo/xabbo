using Xabbo;
using Xabbo.Messages;
using Xabbo.Extension;
using Xabbo.Interceptor;

namespace Xabbo.Command;

public abstract class CommandModule
{
    public bool IsAvailable { get; set; }
    public CommandManager Commands { get; private set; } = null!;

    protected IExtension Ext => Commands.Extension;
    protected IMessageDispatcher Dispatcher => Ext.Dispatcher;
    protected IMessageManager Messages => Ext.Messages;

    public CancellationToken DisconnectToken => Ext.DisconnectToken;
    public ClientType Client => Ext.Session.Client.Type;
    public Session Session => Ext.Session;
    public Hotel Hotel => Ext.Session.Hotel;
    public bool IsConnected => Ext.IsConnected;

    public CommandModule() { }

    public void Initialize(CommandManager manager)
    {
        Commands = manager;

        OnInitialize();
    }

    protected virtual void OnInitialize() { IsAvailable = true; }

    protected void ShowMessage(string message) => Commands.ShowMessage(message);
}
