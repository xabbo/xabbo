namespace Xabbo.Command;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandModuleAttribute : Attribute
{
    public ClientType SupportedClients { get; set; } = ClientType.All;
}