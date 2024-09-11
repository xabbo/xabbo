using Xabbo.Messages;

namespace b7.Xabbo.Commands;

public class CommandBinding
{
    public CommandModule? Module { get; }
    public string CommandName { get; }
    public IReadOnlyList<string> Aliases { get; }
    public string? Usage { get; }
    public CommandHandler Handler { get; }

    public IReadOnlyList<Type> UnavailableDependencies { get; set; }
    public Identifiers UnresolvedHeaders { get; set; }

    public CommandBinding(CommandModule? module, string commandName, IEnumerable<string> aliases, string? usage,
        CommandHandler handler)
    {
        Module = module;
        CommandName = commandName;
        Aliases = aliases.ToList().AsReadOnly();
        Usage = usage;
        Handler = handler;
    }
}

public delegate Task CommandHandler(CommandArgs args);
