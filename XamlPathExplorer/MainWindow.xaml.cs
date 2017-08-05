using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace XamlPathExplorer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        private void wrapPanel_DragEnter(object sender, DragEventArgs e) {
            wrapPanel.Style = (Style)FindResource("DragEnter");
        }

        private void wrapPanel_DragLeave(object sender, DragEventArgs e) {
            wrapPanel.Style = (Style)FindResource("DragLeave");
        }

        private void wrapPanel_Drop(object sender, DragEventArgs e) {
            wrapPanel.Style = (Style)FindResource("DragLeave");

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadGeometryInBackgroundThreadFrom(files);
            }
        }

        private void LoadGeometryInBackgroundThreadFrom(string[] files) {
            var worker = new GeometryBackgroundWorker();
            worker.ProgressChanged += Worker_ProgressChanged;

            wrapPanel.Children.Clear();
            worker.RunWorkerAsync(files);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            var pathGeometry = e.UserState as string;
            LoadGeometry(pathGeometry);
        }

        private void LoadGeometry(string geometry) {
            wrapPanel.Children.Add(new PathButton { PathGeometry = Geometry.Parse(geometry) });
        }

    }
}
