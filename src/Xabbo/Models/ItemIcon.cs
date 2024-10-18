using Xabbo.Abstractions;

namespace Xabbo.Models;

public sealed record ItemIcon(int? Revision, string? Identifier, string? Variant) : IItemIcon;