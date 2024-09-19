using System.Collections;
using System.Collections.Immutable;

using Xabbo.Core;

namespace Xabbo.Command;

public sealed record CommandArgs(
    string Command, IEnumerable<string> args,
    ChatType ChatType, int BubbleStyle, string? Recipient
)
    : IReadOnlyList<string>
{
    private readonly ImmutableArray<string> _args = args.ToImmutableArray();

    int IReadOnlyCollection<string>.Count => _args.Length;
    public int Length => _args.Length;

    public string this[int index] => _args[index];

    IEnumerator<string> IEnumerable<string>.GetEnumerator() => _args.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)this).GetEnumerator();
}
