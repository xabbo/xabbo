using Xabbo;

namespace Xabbo.Ext.Core.Commands;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandModuleAttribute : Attribute
{
    public ClientType SupportedClients { get; set; } = ClientType.All;
}