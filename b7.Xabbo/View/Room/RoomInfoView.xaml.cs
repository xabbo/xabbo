using System;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace b7.Xabbo.View
{
    public partial class RoomInfoView : UserControl
    {
        public RoomInfoView()
        {
            InitializeComponent();

            // Loaded += RoomInfoView_Loaded;
        }

        private void RoomInfoView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= RoomInfoView_Loaded;

            foreach (var child in gridInfo.Children)
            {
                if (child is UIElement element)
                {
                    int col = Grid.GetColumn(element);
                    if (col == 1)
                    {
                        Binding binding = new Binding();
                        binding.Path = new PropertyPath("IsInRoom");
                        binding.Converter = new BooleanToVisibilityConverter();
                        binding.Mode = BindingMode.OneWay;
                        BindingOperations.SetBinding(element, VisibilityProperty, binding);
                    }
                }
            }
        }
    }
}
