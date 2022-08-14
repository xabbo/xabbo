using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class GameDataPage : Page
{
    public FurniDataViewManager FurniData { get; }

    public GameDataPage(FurniDataViewManager furniData)
    {
        FurniData = furniData;

        DataContext = this;
        InitializeComponent();
    }
}
