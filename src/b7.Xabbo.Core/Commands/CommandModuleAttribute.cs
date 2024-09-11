using Xabbo;

namespace b7.Xabbo.Core.Commands;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandModuleAttribute : Attribute
{
    public ClientType SupportedClients { get; set; } = ClientType.All;
}