using System;

using GalaSoft.MvvmLight;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Extension;

namespace b7.Xabbo.ViewModel;

public abstract class ComponentViewModel : ObservableObject, IMessageHandler
{
    public IExtension Extension { get; }

    protected Incoming In => Extension.Messages.In;
    protected Outgoing Out => Extension.Messages.Out;

    public ComponentViewModel(IExtension extension)
    {
        Extension = extension;
        Extension.Connected += OnGameConnected;
    }

    private void OnGameConnected(object? sender, GameConnectedEventArgs e)
    {
        Extension.Bind(this);
    }
}
