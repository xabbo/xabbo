using Xabbo.Core;

namespace Xabbo.ViewModels;

public class ChatMessageViewModel : ChatLogEntryViewModel
{
    public string? FigureString { get; set; }
    public required ChatType Type { get; init; }
    public required string Name { get; init; }
    public required string Message { get; init; }
    public required int BubbleStyle { get; init; }

    public bool IsTalk => Type is ChatType.Talk;
    public bool IsShout => Type is ChatType.Shout;
    public bool IsWhisper => Type is ChatType.Whisper;

    public override string ToString() => $"{Name}: {H.RenderText(Message)}";
}