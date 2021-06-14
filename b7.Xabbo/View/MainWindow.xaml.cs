using System;
using System.Windows;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View
{
    public partial class MainWindow : Window
    {
        private readonly MainViewManager _viewModel;

        public MainWindow(MainViewManager viewModel)
        {
            DataContext = _viewModel = viewModel;
            Initialized += MainWindow_Initialized;

            InitializeComponent();
        }

        private async void MainWindow_Initialized(object? sender, EventArgs e)
        {
            Initialized -= MainWindow_Initialized;

            await _viewModel.InitializeAsync();
        }
    }
}
