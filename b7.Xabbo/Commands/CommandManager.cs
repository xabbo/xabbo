using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Interceptor.Dispatcher;

using Xabbo.Core;
using Xabbo.Core.Game;

using b7.Xabbo.Components;

namespace b7.Xabbo.Commands
{
    public class CommandManager
    {
        public const string PREFIX = "/";

        private bool _isInitialized = false;

        private readonly Dictionary<string, CommandBinding> _bindings;

        private readonly CommandModule[] _modules;

        private readonly ProfileManager _profileManager;
        private readonly RoomManager _roomManager;
        private readonly XabboUserComponent _xabboUser;

        public bool IsAvailable { get; private set; }

        public IInterceptor Interceptor { get; }
        private IInterceptDispatcher Dispatcher => Interceptor.Dispatcher;
        private IMessageManager Messages => Interceptor.Messages;
        private Incoming In => Messages.In;
        private Outgoing Out => Messages.Out;

        public CommandManager(IInterceptor interceptor,
            IEnumerable<CommandModule> modules,
            ProfileManager profileManager,
            RoomManager roomManager,
            XabboUserComponent xabboUser)
        {
            _bindings = new Dictionary<string, CommandBinding>(StringComparer.OrdinalIgnoreCase);

            Interceptor = interceptor;
            _modules = modules.ToArray();
            _profileManager = profileManager;
            _roomManager = roomManager;
            _xabboUser = xabboUser;

            Interceptor.Connected += OnConnected;
        }

        private void OnConnected(object? sender, GameConnectedEventArgs e)
        {
            Initialize();

            Interceptor.Dispatcher.Bind(this, Interceptor.ClientType);

            foreach (CommandModule module in _modules)
            {
                try
                {
                    Dispatcher.Bind(module, Interceptor.ClientType);
                    module.IsAvailable = true;
                }
                catch
                {
                    module.IsAvailable = false;
                }
            }
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                InitializeModules();
                _isInitialized = true;
            }
        }

        protected void InitializeModules()
        {
            Register(OnHelp, "help");

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (CommandModule module in _modules)
            {
                Type type = module.GetType();

                foreach (var method in type.GetMethods(bindingFlags))
                {
                    var commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                    if (commandAttribute == null) continue;

                    var handler = (CommandHandler)method.CreateDelegate(typeof(CommandHandler), module);
                    var binding = new CommandBinding(
                        module,
                        commandAttribute.CommandName,
                        commandAttribute.Aliases,
                        commandAttribute.Usage,
                        handler
                    );

                    foreach (string commandName in new[] { commandAttribute.CommandName }.Concat(commandAttribute.Aliases))
                    {
                        if (_bindings.ContainsKey(commandName))
                            throw new InvalidOperationException($"The command '{commandName}' is already registered");
                        _bindings.Add(commandName, binding);
                    }
                }

                module.Initialize(this);
            }
        }

        private Task OnHelp(CommandArgs args)
        {
            if (args.Count > 0) return Task.CompletedTask;

            foreach (var commandGroup in _bindings.Values.GroupBy(x => x.Handler.Target))
            {
                if (commandGroup.Key == this) continue;

                string groupName = commandGroup.Key?.GetType().Name ?? "<unknown>";
                var commandNames = commandGroup
                    .Distinct()
                    .SelectMany(x =>
                        new[] { x.CommandName }
                        .Concat(x.Aliases)
                    );

                ShowMessage($"{groupName}: {string.Join(", ", commandNames)}");
            }

            return Task.CompletedTask;
        }

        public void Register(CommandHandler handler, string commandName, string? usage = null, params string[] aliases)
        {
            var binding = new CommandBinding(null, commandName, aliases, usage, handler);
            var commandNames = aliases.Concat(new[] { commandName });
            var boundCommands = commandNames.Where(x => _bindings.ContainsKey(x)).ToList();
            if (boundCommands.Any())
                throw new InvalidOperationException($"Command(s) are already registered: {string.Join(", ", boundCommands)}");

            foreach (string name in commandNames)
                _bindings.Add(name, binding);
        }

        [RequiredIn(nameof(Incoming.Whisper))]
        public void ShowMessage(string message)
        {
            _xabboUser.ShowMessage(message);
        }

        [InterceptOut(
            nameof(Outgoing.Whisper),
            nameof(Outgoing.Chat),
            nameof(Outgoing.Shout)
        )]
        private async void HandleChat(InterceptArgs e)
        {
            var packet = e.OriginalPacket;

            ChatType chatType;
            if (packet.Header == Out.Whisper) chatType = ChatType.Whisper;
            else if (packet.Header == Out.Chat) chatType = ChatType.Talk;
            else if (packet.Header == Out.Shout) chatType = ChatType.Shout;
            else return;

            string message = packet.ReadString().Trim();
            int bubbleStyle = packet.ReadInt();

            string? whisperTarget = null;

            if (packet.Header == Out.Whisper)
            {
                int index = message.IndexOf(' ');
                if (index > 0)
                {
                    whisperTarget = message.Substring(0, index);
                    message = message.Substring(index + 1).Trim();
                }
            }

            if (!message.StartsWith(PREFIX)) return;

            e.Block();

            message = message.Substring(PREFIX.Length);
            if (string.IsNullOrWhiteSpace(message)) return;

            string[] split = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string command = split[0].ToLower();
            string[] args = split.Skip(1).ToArray();

            if (string.IsNullOrWhiteSpace(command)) return;

            if (!_bindings.TryGetValue(command, out CommandBinding? binding)) return;

            if (binding.Module != null && !binding.Module.IsAvailable)
            {
                ShowMessage($"Command module '{binding.Module.GetType().Name}' is unavailable");
                return;
            }
            
            try
            {
                await binding.Handler.Invoke(new CommandArgs(command, args, chatType, bubbleStyle, whisperTarget));
            }
            catch (Exception? ex)
            {
                bool unexpected = false;
                if (ex is TargetInvocationException t)
                {
                    ex = t.InnerException;
                    unexpected = true;
                }

                ShowMessage("An error occurred while executing that command");

                if (ex is null) return;
                Debug.WriteLine($"[CommandManager] An {(unexpected ? "unexpected " : "")}error occurred while executing command handler! " +
                    $"({binding.Handler.Target?.GetType().FullName ?? "?"}.{binding.Handler.Method.Name})\r\n" +
                    $"{ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
}
