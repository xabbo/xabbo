using System;

using MaterialDesignExtensions.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View
{
    public partial class MainWindow : MaterialWindow
    {
        public MainWindow(MainViewManager mainViewManager)
        {
            DataContext = mainViewManager;

            InitializeComponent();
        }
    }
}
