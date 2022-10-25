using System;

using GalaSoft.MvvmLight;

using Xabbo;
using Xabbo.Messages;
using Xabbo.Extension;

namespace b7.Xabbo.Components;

public abstract class Component : ObservableObject, IMessageHandler
{
    protected IExtension Extension { get; }
    protected ClientType Client => Extension.Client;
    protected IMessageManager Messages => Extension.Messages;
    protected Incoming In => Extension.Messages.In;
    protected Outgoing Out => Extension.Messages.Out;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => Set(ref _isActive, value);
    }

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
