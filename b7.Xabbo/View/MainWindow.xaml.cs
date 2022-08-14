using System;
using System.Windows;
using System.Windows.Controls;

using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

using b7.Xabbo.ViewModel;
using Wpf.Ui.Controls;

namespace b7.Xabbo.View
{
    public partial class MainWindow : Window, INavigationWindow
    {
        public MainWindow(MainViewManager manager,
            INavigationService navigation,
            IPageService pageService)
        {
            DataContext = manager;

            InitializeComponent();

            navigation.SetNavigation(rootNavigation);
            SetPageService(pageService);

            rootNavigation.PageService = manager.PageService;
        }

        public void CloseWindow() => Close();

        public Frame GetFrame() => rootFrame;

        public INavigation GetNavigation() => rootNavigation;

        public bool Navigate(Type pageType) => rootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => rootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        private void ButtonPin_Click(object sender, RoutedEventArgs e) => Topmost = !Topmost;
    }
}
