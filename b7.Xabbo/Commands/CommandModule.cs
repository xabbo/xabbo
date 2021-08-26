using System;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Core;
using Xabbo.Core.Tasks;
using Xabbo.Interceptor;
using Xabbo.Interceptor.Dispatcher;
using Xabbo.Interceptor.Tasks;
using Xabbo.Messages;

namespace b7.Xabbo.Commands
{
    public abstract class CommandModule
    {
        public bool IsAvailable { get; set; }
        public CommandManager Commands { get; private set; } = null!;

        protected IInterceptor Interceptor => Commands.Interceptor;
        protected IInterceptDispatcher Dispatcher => Interceptor.Dispatcher;
        protected IMessageManager Messages => Interceptor.Messages;
        protected Incoming In => Messages.In;
        protected Outgoing Out => Messages.Out;

        protected void Send(Header header, params object[] values) => Interceptor.Send(header, values);
        protected void Send(IReadOnlyPacket packet) => Interceptor.Send(packet);

        public CommandModule() { }

        public void Initialize(CommandManager manager)
        {
            Commands = manager;

            OnInitialize();
        }

        protected virtual void OnInitialize() { IsAvailable = true; }

        protected void ShowMessage(string message) => Commands.ShowMessage(message);

        protected Task<IPacket> CaptureAsync(Destination destination, Header[] headers,
            int timeout, CancellationToken cancellationToken, bool block)
        {
            return new CaptureMessageTask(Interceptor, destination, block, headers)
                .ExecuteAsync(timeout, cancellationToken);
        }

        protected Task<IPacket> ReceiveAsync(Header header, int timeout = 10000,
            CancellationToken cancellationToken = default, bool block = false)
        {
            return ReceiveAsync(new[] { header }, timeout, cancellationToken, block);
        }

        protected Task<IPacket> ReceiveAsync(Header[] headers, int timeout = 10000,
            CancellationToken cancellationToken = default, bool block = false)
        {
            return CaptureAsync(Destination.Client, headers, timeout, cancellationToken, block);
        }

        protected Task<IPacket> CaptureOutAsync(Header header, int timeout = 10000,
            CancellationToken cancellationToken = default, bool block = false)
        {
            return CaptureOutAsync(new[] { header }, timeout, cancellationToken, block);
        }

        protected Task<IPacket> CaptureOutAsync(Header[] headers, int timeout = 10000,
            CancellationToken cancellationToken = default, bool block = false)
        {
            return CaptureAsync(Destination.Server, headers, timeout, cancellationToken, block);
        }
    }
}
