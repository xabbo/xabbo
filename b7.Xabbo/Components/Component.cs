using System;

using GalaSoft.MvvmLight;

using Xabbo.Common;
using Xabbo.Messages;
using Xabbo.Interceptor;

namespace b7.Xabbo.Components
{
    public abstract class Component : ObservableObject, IInterceptHandler
    {
        protected IInterceptor Interceptor { get; }
        protected ClientType Client => Interceptor.Client;
        protected IMessageManager Messages => Interceptor.Messages;
        protected Incoming In => Interceptor.Messages.In;
        protected Outgoing Out => Interceptor.Messages.Out;

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
                Interceptor.Dispatcher.Bind(this, Interceptor.Client);
            }
        }

        protected virtual void OnDisconnected(object? sender, EventArgs e)
        {
            Interceptor.Dispatcher.Release(this);
        }
    }
}
