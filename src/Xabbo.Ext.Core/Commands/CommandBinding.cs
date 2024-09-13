using Xabbo.Messages;

namespace Xabbo.Ext.Commands;

public class CommandBinding
{
    public CommandModule? Module { get; }
    public string CommandName { get; }
    public IReadOnlyList<string> Aliases { get; }
    public string? Usage { get; }
    public ClientType SupportedClients { get; }
    public CommandHandler Handler { get; }

    public IReadOnlyList<Type> UnavailableDependencies { get; set; } = [];
    public Identifiers UnresolvedHeaders { get; set; } = [];

    public CommandBinding(CommandModule? module, string commandName, IEnumerable<string> aliases,
        string? usage, ClientType supportedClients, CommandHandler handler)
    {
        Module = module;
        CommandName = commandName;
        Aliases = aliases.ToList().AsReadOnly();
        Usage = usage;
        SupportedClients = supportedClients;
        Handler = handler;
    }
}

public delegate Task CommandHandler(CommandArgs args);
