using System.ComponentModel;

using Xabbo;
using Xabbo.Messages;
using Xabbo.Extension;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace b7.Xabbo.Components;

public abstract class Component : ReactiveObject, IMessageHandler
{
    protected void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        this.RaiseAndSetIfChanged(ref field, value, propertyName);
    }

    protected IExtension Extension { get; }
    protected ClientType Client => Extension.Client;
    protected IMessageManager Messages => Extension.Messages;
    protected Incoming In => Extension.Messages.In;
    protected Outgoing Out => Extension.Messages.Out;

    [Reactive] public bool IsActive { get; set; }

    public Component(IExtension extension)
    {
        Extension = extension;
        Extension.Initialized += OnInitialized;
        Extension.Connected += OnConnected;
        Extension.Disconnected += OnDisconnected;
    }

    protected virtual void OnInitialized(object? sender, ExtensionInitializedEventArgs e) { }

    protected virtual void OnConnected(object? sender, EventArgs e)
    {
        if (!Extension.Dispatcher.IsBound(this))
        {
            Extension.Dispatcher.Bind(this, Extension.Client);
        }
    }

    protected virtual void OnDisconnected(object? sender, EventArgs e)
    {
        Extension.Dispatcher.Release(this);
    }
}
