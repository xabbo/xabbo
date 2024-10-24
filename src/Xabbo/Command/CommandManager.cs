using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

using Xabbo.Messages;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Components;
using Xabbo.Exceptions;

namespace Xabbo.Command;

[Intercept]
public partial class CommandManager
{
    public const string CommandPrefix = "/";

    private readonly ILogger Log;
    private bool _isInitialized = false;

    private readonly Dictionary<string, CommandBinding> _bindings;

    private readonly CommandModule[] _modules;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;
    private readonly XabbotComponent _xabbot;

    public bool IsAvailable { get; private set; }

    public IExtension Extension { get; }
    private IMessageDispatcher Dispatcher => Extension.Dispatcher;
    private IMessageManager Messages => Extension.Messages;

    public CommandManager(
        ILoggerFactory loggerFactory,
        IExtension extension,
        IEnumerable<CommandModule> modules,
        ProfileManager profileManager,
        RoomManager roomManager,
        XabbotComponent xabbot)
    {
        Log = loggerFactory.CreateLogger<CommandManager>();

        _bindings = new Dictionary<string, CommandBinding>(StringComparer.OrdinalIgnoreCase);

        Extension = extension;
        _modules = modules.ToArray();
        _profileManager = profileManager;
        _roomManager = roomManager;
        _xabbot = xabbot;

        Extension.Connected += OnConnected;

        Log.LogInformation("Loaded {n} command modules.", _modules.Length);
    }

    private void OnConnected(ConnectedEventArgs e)
    {
        Initialize();

        if (this is IMessageHandler handler)
            handler.Attach(Extension);

        foreach (CommandModule module in _modules)
        {
            try
            {
                if (module is IMessageHandler moduleHandler)
                    moduleHandler.Attach(Extension);
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

            var moduleAttribute = type.GetCustomAttribute<CommandModuleAttribute>()
                ?? throw new Exception($"Command module '{type.Name}' must be decorated with CommandModuleAttribute.");

            foreach (var method in type.GetMethods(bindingFlags))
            {
                var commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute == null) continue;

                // TODO Validate method signature
                CommandHandler handler;
                try
                {
                    handler = (CommandHandler)method.CreateDelegate(typeof(CommandHandler), module);
                }
                catch (Exception ex)
                {
                    Log.LogError("Failed to bind command '{CommandName}': {Message}",
                        commandAttribute.CommandName, ex.Message);
                    continue;
                }

                var binding = new CommandBinding(
                    module,
                    commandAttribute.CommandName,
                    [.. commandAttribute.Aliases],
                    commandAttribute.Usage,
                    moduleAttribute.SupportedClients & commandAttribute.SupportedClients & ClientType.All,
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
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/xabbo/xabbo#commands",
                UseShellExecute = true
            });
        }
        catch { }

        return Task.CompletedTask;
    }

    private void Register(CommandHandler handler, string commandName, string? usage = null,
        ClientType supportedClients = ClientType.All, params string[] aliases)
    {
        var binding = new CommandBinding(null, commandName, [.. aliases], usage, supportedClients, handler);
        var commandNames = aliases.Concat([commandName]);
        var boundCommands = commandNames.Where(x => _bindings.ContainsKey(x)).ToList();
        if (boundCommands.Count != 0)
            throw new InvalidOperationException($"Command(s) are already registered: {string.Join(", ", boundCommands)}");

        foreach (string name in commandNames)
            _bindings.Add(name, binding);
    }

    public void ShowMessage(string message)
    {
        _xabbot.ShowMessage(message);
    }

    [Intercept]
    private void HandleChat(Intercept e, ChatMsg chat)
    {
        if (!chat.Message.StartsWith(CommandPrefix)) return;

        e.Block();

        string message = chat.Message[CommandPrefix.Length..];
        if (string.IsNullOrWhiteSpace(message)) return;

        string[] split = message.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length == 0) return;

        var (command, args) = (split[0].ToLower(), split[1..]);

        #if DEBUG
        if (command == "throw")
            throw new InvalidOperationException("Test exception.");
        #endif

        if (!_bindings.TryGetValue(command, out CommandBinding? binding)) return;

        if (binding.Module is { IsAvailable: false })
        {
            ShowMessage($"Command module '{binding.Module.GetType().Name}' is unavailable");
            return;
        }

        ClientType supportedClients = binding.SupportedClients;
        if (binding.Module is not null)
            supportedClients &= binding.Module.Client;

        if ((supportedClients & Extension.Session.Client.Type) is ClientType.None)
        {
            ShowMessage($"The `{command}` command is not supported on {Extension.Session.Client.Type}.");
            return;
        }

        Task.Run(() => ExecuteCommand(binding, command, args, chat.Type, chat.BubbleStyle, chat.Recipient));
    }

    private async Task ExecuteCommand(CommandBinding binding, string command, string[] args, ChatType chatType, int bubbleStyle, string whisperTarget)
    {
        try
        {
            await binding.Handler.Invoke(new CommandArgs(command, args, chatType, bubbleStyle, whisperTarget));
        }
        catch (OperationInProgressException ex)
        {
            ShowMessage($"Operation '{ex.OperationName}' is already in progress.");
        }
        catch (Exception? ex)
        {
            if (ex is TargetInvocationException t)
                ex = t.InnerException;

            if (ex is InvalidArgsException && !string.IsNullOrWhiteSpace(binding.Usage))
            {
                ShowMessage($"Usage: /{command} {binding.Usage}");
                return;
            }

            if (ex is TimeoutException)
            {
                ShowMessage($"/{command}: The request timed out.");
                return;
            }

            ShowMessage($"/{command}: An error occurred.");

            if (ex is null) return;

            Log.LogError(ex,
                "An error occurred while executing a command handler ({Handler}:{Method}). {Message}",
                binding.Handler.Target?.GetType().FullName ?? "?",
                binding.Handler.Method.Name,
                ex.Message
            );
        }

    }
}
