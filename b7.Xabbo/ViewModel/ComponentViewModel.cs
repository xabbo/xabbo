using System;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.ViewModel
{
    public abstract class ComponentViewModel : ObservableObject
    {
        public IInterceptor Interceptor { get; }

        protected Incoming In => Interceptor.Messages.In;
        protected Outgoing Out => Interceptor.Messages.Out;

        protected void Send(Header header, params object[] values) => Interceptor.Send(header, values);
        protected void Send(IReadOnlyPacket packet) => Interceptor.Send(packet);
        protected Task SendAsync(Header header, params object[] values) => Interceptor.SendAsync(header, values);
        protected Task SendAsync(IReadOnlyPacket packet) => Interceptor.SendAsync(packet);

        public ComponentViewModel(IInterceptor interceptor)
        {
            Interceptor = interceptor;
            Interceptor.Connected += Interceptor_Connected;
        }

        private void Interceptor_Connected(object? sender, GameConnectedEventArgs e)
        {
            Interceptor.Dispatcher.Bind(this);
        }
    }
}
