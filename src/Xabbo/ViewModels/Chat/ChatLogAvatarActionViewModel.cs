namespace Xabbo.ViewModels;

public sealed class ChatLogAvatarActionViewModel : ChatLogEntryViewModel
{
    public override bool IsSelectable => false;
    public required string UserName { get; init; }
    public required string Action { get; init; }

    public override string ToString() => $"* {UserName} {Action}";
}