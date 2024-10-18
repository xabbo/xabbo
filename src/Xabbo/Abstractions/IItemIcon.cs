namespace Xabbo.Abstractions;

public interface IItemIcon
{
    int? Revision { get; }
    string? Identifier { get; }
    string? Variant { get; }
}