using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;

namespace XamlPathExplorer {
    /// <summary>
    /// Interaction logic for PathButton.xaml
    /// </summary>
    public partial class PathButton : UserControl {
        public PathButton() {
            InitializeComponent();
            DataContext = this;
        }

        private static readonly Geometry DefaultPathGeometry = Geometry.Parse("M9.5999776,12.699997L11.899997,15.099998 13.299991,14.300003 19.000005,20 22.899999,18 28.299995,20.699997 28.299995,25.699997 3.5999767,25.699997 3.5999767,18.599998z M20.500005,11.699997L21.199987,11.699997 21.000005,12.800003 20.699987,12.800003z M23.09998,10.699997L23.799993,11.599998 23.59998,11.800003 22.699987,11.099998z M18.699987,10.699997L19.199987,11.199997 18.299993,11.900002 18.000005,11.599998z M23.59998,8.5999985L24.699987,8.8000031 24.699987,9 23.59998,9.1999969z M18.09998,8.5999985L18.09998,9.3000031 17.000005,9 17.000005,8.8000031z M20.799993,7C21.899999,7 22.699987,7.9000015 22.699987,8.9000015 22.699987,10 21.799993,10.800003 20.799993,10.800003 19.699987,10.800003 18.899999,9.9000015 18.899999,8.9000015 18.899999,7.8000031 19.799993,7 20.799993,7z M23.500005,6.0999985L23.699987,6.3000031 23.000005,7.1999969 22.500005,6.6999969z M18.199987,6.0999985L19.09998,6.8000031 18.59998,7.3000031 18.000005,6.3000031z M20.699987,5L21.000005,5 21.199987,6.0999985 20.500005,6.0999985z M2.1999823,2.4000015L2.1999823,26.800003 29.400001,26.800003 29.400001,2.4000015z M0,0L31.900001,0 31.900001,32 0,32z");

        private PathDetails _pathDetails;
        public PathDetails PathDetails {
            get { return _pathDetails; }
            set {
                _pathDetails = value;
                PathGeometry = Geometry.Parse(_pathDetails.Geometry);
            }
        }

        public Geometry PathGeometry {
            get { return (Geometry)GetValue(PathGeometryProperty); }
            set { SetValue(PathGeometryProperty, value); }
        }

        public static readonly DependencyProperty PathGeometryProperty =
            DependencyProperty.Register("PathGeometry", typeof(Geometry),
                typeof(PathButton), new UIPropertyMetadata(DefaultPathGeometry));

        public TextEditor Editor { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (PathDetails == null) {
                System.Windows.Forms.MessageBox.Show("This is just a default example...");
                return;
            }
            // System.Windows.Forms.MessageBox.Show("Loaded from: " + PathDetails.File.FullName );
            using (var stream = PathDetails.File.OpenRead()) {
                Editor.Load(stream);
            }

            // Editor.ScrollTo(80, 5);
            Editor.Select(PathDetails.StartingIndex, PathDetails.Length);
        }
    }
}
