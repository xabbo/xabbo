using System.Collections.Immutable;

using Xabbo.Messages;

namespace Xabbo.Command;

public sealed record CommandBinding(
    CommandModule? Module,
    string CommandName,
    ImmutableArray<string> Aliases,
    string? Usage,
    ClientType SupportedClients,
    CommandHandler Handler)
{
    public IReadOnlyList<Type> UnavailableDependencies { get; set; } = [];
    public Identifiers UnresolvedHeaders { get; set; } = [];
}

public delegate Task CommandHandler(CommandArgs args);
