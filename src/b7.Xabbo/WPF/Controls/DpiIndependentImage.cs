using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace b7.Xabbo.WPF.Controls;

public class DpiIndependentImage : Image
{
    public DpiIndependentImage()
    {
        Stretch = System.Windows.Media.Stretch.Uniform;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        if (Source is BitmapFrame frame)
        {
            return new Size(frame.PixelWidth, frame.PixelHeight);
        }

        return base.MeasureOverride(constraint);
    }
}
