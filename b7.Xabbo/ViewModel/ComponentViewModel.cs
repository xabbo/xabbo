using System;

using GalaSoft.MvvmLight;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.ViewModel
{
    public abstract class ComponentViewModel : ObservableObject, IInterceptHandler
    {
        public IInterceptor Interceptor { get; }

        protected Incoming In => Interceptor.Messages.In;
        protected Outgoing Out => Interceptor.Messages.Out;

        public ComponentViewModel(IInterceptor interceptor)
        {
            Interceptor = interceptor;
            Interceptor.Connected += Interceptor_Connected;
        }

        private void Interceptor_Connected(object? sender, GameConnectedEventArgs e)
        {
            Interceptor.Bind(this);
        }
    }
}
