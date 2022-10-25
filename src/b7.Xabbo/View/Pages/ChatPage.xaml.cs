using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class ChatPage : Page
{
    public ChatPage(ChatLogViewManager manager)
    {
        DataContext = manager;

        InitializeComponent();
    }
}
