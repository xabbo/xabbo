#if DEBUG
namespace Xabbo.ViewModels;

internal partial class DesignViewModels
{
    public static readonly GiftViewModel GiftViewModel = new()
    {
        Message = "Happy birthday!",
        SenderName = "Someone",
        ItemName = "Birthday Cake",
    };
}
#endif
