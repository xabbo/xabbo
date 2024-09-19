namespace Xabbo.Command;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class CommandAttribute(string commandName, params string[] aliases) : Attribute
{
    public string CommandName { get; } = commandName;
    public IReadOnlyList<string> Aliases { get; } = aliases;

    public string Usage { get; set; } = "";
    public ClientType SupportedClients { get; set; } = ClientType.All;
}
