namespace Xabbo.ViewModels;

public sealed class ChatLogRoomEntryViewModel : ChatLogEntryViewModel
{
    public override bool IsSelectable => false;
    public required string RoomName { get; init; }
    public required string RoomOwner { get; init; }
}