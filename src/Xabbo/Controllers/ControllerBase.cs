using ReactiveUI;
using Xabbo.Extension;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace Xabbo.Controllers;

public abstract partial class ControllerBase : ReactiveObject, IInterceptorContext
{
    private readonly IExtension _extension;

    IInterceptor IInterceptorContext.Interceptor => _extension;

    protected Session Session => _extension.Session;

    protected IExtension Ext => _extension;

    public ControllerBase(IExtension extension)
    {
        _extension = extension;
        _extension.Connected += OnConnected;
    }

    protected virtual void OnConnected(ConnectedEventArgs e)
    {
        if (this is IMessageHandler handler)
            handler.Attach(_extension);
    }
}
