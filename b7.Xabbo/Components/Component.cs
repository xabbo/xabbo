using System;

using GalaSoft.MvvmLight;

using Xabbo;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public abstract class Component : ObservableObject
    {
        protected IInterceptor Interceptor { get; }
        protected ClientType ClientType => Interceptor.ClientType;
        protected IMessageManager Messages => Interceptor.Messages;
        protected Incoming In => Interceptor.Messages.In;
        protected Outgoing Out => Interceptor.Messages.Out;
        protected void Send(Header header, params object[] values) => Interceptor.Send(header, values);
        protected void Send(IReadOnlyPacket packet) => Interceptor.Send(packet);

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }

        public Component(IInterceptor interceptor)
        {
            Interceptor = interceptor;
            Interceptor.Initialized += OnInitialized;
            Interceptor.Connected += OnConnected;
            Interceptor.Disconnected += OnDisconnected;
        }

        protected virtual void OnInitialized(object? sender, InterceptorInitializedEventArgs e) { }

        protected virtual void OnConnected(object? sender, EventArgs e)
        {
            if (!Interceptor.Dispatcher.IsBound(this))
            {
                Interceptor.Dispatcher.Bind(this);
            }
        }

        protected virtual void OnDisconnected(object? sender, EventArgs e)
        {
            Interceptor.Dispatcher.Release(this);
        }
    }
}
