using System.Diagnostics;
using System.Reflection;

using Xabbo;
using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;

using b7.Xabbo.Components;

namespace b7.Xabbo.Commands;

[Intercept]
public partial class CommandManager
{
    public const string CommandPrefix = "/";

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

    public CommandManager(IExtension extension,
        IEnumerable<CommandModule> modules,
        ProfileManager profileManager,
        RoomManager roomManager,
        XabbotComponent xabbot)
    {
        _bindings = new Dictionary<string, CommandBinding>(StringComparer.OrdinalIgnoreCase);

        Extension = extension;
        _modules = modules.ToArray();
        _profileManager = profileManager;
        _roomManager = roomManager;
        _xabbot = xabbot;

        Extension.Connected += OnConnected;
    }

    public IDisposable Attach(IInterceptor interceptor)
    {
        return null!;
    }

    private void OnConnected(GameConnectedArgs e)
    {
        Initialize();

        Attach(Extension);

        foreach (CommandModule module in _modules)
        {
            try
            {
                if (module is IMessageHandler handler)
                    handler.Attach(Extension);
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

                // TODO Validate method signature
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
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/b7c/b7.Xabbo#commands",
                UseShellExecute = true
            });
        }
        catch { }

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

        string command = split[0].ToLower();
        string[] args = split[1..];

        if (!_bindings.TryGetValue(command, out CommandBinding? binding)) return;

        if (binding.Module != null && !binding.Module.IsAvailable)
        {
            ShowMessage($"Command module '{binding.Module.GetType().Name}' is unavailable");
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
        catch (Exception? ex)
        {
            bool unexpected = false;
            if (ex is TargetInvocationException t)
            {
                ex = t.InnerException;
                unexpected = true;
            }

            if (ex is InvalidArgsException && !string.IsNullOrWhiteSpace(binding.Usage))
            {
                ShowMessage($"Usage: /{command} {binding.Usage}");
                return;
            }

            ShowMessage("An error occurred while executing that command");

            if (ex is null) return;
            Debug.WriteLine($"[CommandManager] An {(unexpected ? "unexpected " : "")}error occurred while executing command handler! " +
                $"({binding.Handler.Target?.GetType().FullName ?? "?"}.{binding.Handler.Method.Name})\r\n" +
                $"{ex.Message}\r\n{ex.StackTrace}");
        }

    }
}
