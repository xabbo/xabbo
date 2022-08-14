using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class RoomPage : Page
{
    public RoomInfoViewManager Info { get; }
    public EntitiesViewManager Entities { get; }
    public VisitorsViewManager Visitors { get; }
    public BanListViewManager BanList { get; }
    public FurniViewManager Furni { get; }

    public RoomPage(
        RoomInfoViewManager info,
        EntitiesViewManager entities,
        VisitorsViewManager visitors,
        BanListViewManager banList,
        FurniViewManager furni)
    {
        DataContext = this;

        Info = info;
        Entities = entities;
        Visitors = visitors;
        BanList = banList;
        Furni = furni;

        InitializeComponent();
    }
}
