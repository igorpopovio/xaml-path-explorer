using System.Windows;
using System.Windows.Controls;

namespace PathButton {
    public class PathButton : Button
    {
        static PathButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathButton),
                new FrameworkPropertyMetadata(typeof(PathButton)));
        }
    }
}
