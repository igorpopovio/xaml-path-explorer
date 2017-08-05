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
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            wrapPanel.Children.Clear();
            worker.RunWorkerAsync(files);
            statusBarItem.Content = $"Starting to load paths from {files}";
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            var count = e.Result as int?;
            statusBarItem.Content = $"Loaded {count} paths";
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
