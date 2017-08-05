using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
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

        private bool IsValidGeometry(string pathGeometry) {
            try {
                Geometry.Parse(pathGeometry);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private int FindForwardDelimiters(FileInfo file, string fileContents, int index, string delimiters) {
            for (int i = index; i < file.Length; i++)
                if (delimiters.Contains(fileContents[i]))
                    return i;
            throw new Exception($"Delimiter not found forward from index {index} in {file.FullName}!");
        }

        private int FindBackwardDelimiters(FileInfo file, string fileContents, int index, string delimiters) {
            for (int i = index; i > 0; i--)
                if (delimiters.Contains(fileContents[i]))
                    return i;
            throw new Exception($"Delimiter not found backward from index {index} in {file.FullName}!");
        }
        
        private void LoadGeometry(string geometry) {
            wrapPanel.Children.Add(new PathButton { PathGeometry = Geometry.Parse(geometry) });
        }

        private void wrapPanel_DragEnter(object sender, DragEventArgs e) {
            wrapPanel.Style = (Style)FindResource("DragEnter");
        }

        private void wrapPanel_DragLeave(object sender, DragEventArgs e) {
            wrapPanel.Style = (Style) FindResource("DragLeave");
        }

        private void wrapPanel_Drop(object sender, DragEventArgs e) {
            wrapPanel.Style = (Style)FindResource("DragLeave");
            var files = new string[] { };
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                // Note that you can have more than one file.
                files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                wrapPanel.Children.Clear();


                var worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true,
                };

                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync(files);
            }
        }

        

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var pathGeometry = e.UserState as string;
            LoadGeometry(pathGeometry);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var files = e.Argument as string[];
            var defaultDirectory = files[0];
            

            var pathGeometryRegex = $@"(?<=[""<>])[MLCV]\s*[-+]?[0-9]*\.?[0-9]+(?:[eE][-+]?[0-9]+)?[,\s]+";
            var regex = new Regex(pathGeometryRegex, RegexOptions.IgnoreCase & RegexOptions.Multiline & RegexOptions.Compiled);

            var directoryInfo = new DirectoryInfo(defaultDirectory);

            foreach (var file in directoryInfo.GetFiles("*.xaml", SearchOption.AllDirectories))
            {
                var fileContents = file.OpenText().ReadToEnd();

                var delimiters = "\"<>";

                var index = 0;
                var shouldContinue = true;

                while (shouldContinue)
                {
                    var match = regex.Match(fileContents, index);
                    if (match.Success)
                    {
                        var startingIndex = FindBackwardDelimiters(file, fileContents, match.Index, delimiters) + 1;
                        var endingIndex = FindForwardDelimiters(file, fileContents, match.Index, delimiters);
                        var pathGeometry = fileContents.Substring(startingIndex, endingIndex - startingIndex);
                        pathGeometry = HttpUtility.HtmlDecode(pathGeometry);
                        if (IsValidGeometry(pathGeometry))
                        {
                            // Thread.Sleep(10);
                            worker.ReportProgress(0, pathGeometry);
                        }
                        else
                            System.Windows.Forms.MessageBox.Show($"Failed to render geometry: {pathGeometry}\n from\n{file.FullName}!");
                        index = endingIndex;
                    }
                    shouldContinue = match.Success;
                }
            }

        }
    }
}
