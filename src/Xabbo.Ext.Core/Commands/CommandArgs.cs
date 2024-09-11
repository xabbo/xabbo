using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xabbo.Core;

namespace Xabbo.Ext.Commands;

public class CommandArgs : EventArgs, IReadOnlyList<string>
{
    private readonly IReadOnlyList<string> _args;

    public string Command { get;  }
    public ChatType ChatType { get; }
    public int BubbleStyle { get; }
    public string? WhisperTarget { get; }

    public int Count => _args.Count;
    public string this[int index] => _args[index];

    public CommandArgs(string command, IEnumerable<string> args,
        ChatType chatType, int bubbleStyle, string? whisperTarget)
    {
        Command = command;
        _args = args.ToList().AsReadOnly();

        ChatType = chatType;
        BubbleStyle = bubbleStyle;
        WhisperTarget = whisperTarget;
    }

    public IEnumerator<string> GetEnumerator() => _args.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
