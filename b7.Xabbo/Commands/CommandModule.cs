using System;
using System.Runtime.CompilerServices;
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

        protected Task<IPacket> ReceiveAsync(Header header, int timeout = -1, bool block = false,
            CancellationToken cancellationToken = default)
        {
            return Interceptor.ReceiveAsync(header, timeout, block, cancellationToken);
        }

        protected Task<IPacket> ReceiveAsync(ITuple tuple, int timeout = -1, bool block = false,
            CancellationToken cancellationToken = default)
        {
            return Interceptor.ReceiveAsync(tuple, timeout, block, cancellationToken);
        }
    }
}
