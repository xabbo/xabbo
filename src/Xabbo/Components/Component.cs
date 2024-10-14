using System.Runtime.CompilerServices;
using ReactiveUI;

using Xabbo.Messages;
using Xabbo.Extension;

namespace Xabbo.Components;

public abstract class Component : ReactiveObject
{
    protected void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        this.RaiseAndSetIfChanged(ref field, value, propertyName);
    }

    protected IExtension Ext { get; }
    protected ClientType Client => Ext.Session.Client.Type;
    protected IMessageManager Messages => Ext.Messages;

    /// <summary>
    /// Gets if this component is available.
    /// </summary>
    [Reactive] public bool IsAvailable { get; set; }

    /// <summary>
    /// Gets the identifiers that failed to resolve for this component, if it is unavailable.
    /// </summary>
    public Identifiers UnresolvedIdentifiers { get; set; } = [];

    protected Session Session => Ext.Session;

    public Component(IExtension ext)
    {
        Ext = ext;
        Ext.Initialized += OnInitialized;
        Ext.Connected += OnConnected;
        Ext.Disconnected += OnDisconnected;
    }

    protected virtual void OnInitialized(InitializedEventArgs e) { }

    protected virtual void OnConnected(ConnectedEventArgs e)
    {
        if (this is IMessageHandler handler)
        {
            try
            {
                handler.Attach(Ext);
                IsAvailable = true;
                UnresolvedIdentifiers = [];
            }
            catch (UnresolvedIdentifiersException ex)
            {
                IsAvailable = false;
                UnresolvedIdentifiers = ex.Identifiers;
            }
        }
    }

    protected virtual void OnDisconnected() { }
}
