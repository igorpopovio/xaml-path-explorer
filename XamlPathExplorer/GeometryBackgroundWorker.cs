using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Media;

namespace XamlPathExplorer {
    public class GeometryBackgroundWorker : BackgroundWorker {
        public GeometryBackgroundWorker() {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
            DoWork += Worker_DoWork;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e) {
            var worker = (BackgroundWorker)sender;
            var files = e.Argument as string[];
            var directory = files[0];
            int count = 0;


            var pathGeometryRegex = $@"(?<=[""<>])[MLCV]\s*[-+]?[0-9]*\.?[0-9]+(?:[eE][-+]?[0-9]+)?[,\s]+";
            var regex = new Regex(pathGeometryRegex, RegexOptions.IgnoreCase & RegexOptions.Multiline & RegexOptions.Compiled);

            var directoryInfo = new DirectoryInfo(directory);
            var delimiters = "\"<>";

            foreach (var file in directoryInfo.GetFiles("*.xaml", SearchOption.AllDirectories)) {
                var fileContents = file.OpenText().ReadToEnd();


                var index = 0;
                var shouldContinue = true;

                while (shouldContinue) {
                    var match = regex.Match(fileContents, index);
                    if (match.Success) {
                        var startingIndex = FindBackwardDelimiters(file, fileContents, match.Index, delimiters) + 1;
                        var endingIndex = FindForwardDelimiters(file, fileContents, match.Index, delimiters);
                        var pathGeometry = fileContents.Substring(startingIndex, endingIndex - startingIndex);
                        pathGeometry = HttpUtility.HtmlDecode(pathGeometry);
                        if (IsValidGeometry(pathGeometry)) {
                            // Thread.Sleep(10);
                            worker.ReportProgress(0, pathGeometry);
                            count++;
                        } else {
                            // FIXME: you can't update the UI from background worker threads
                            // use https://stackoverflow.com/questions/1044460/unhandled-exceptions-in-backgroundworker
                            System.Windows.Forms.MessageBox.Show($"Failed to render geometry: {pathGeometry}\n from\n{file.FullName}!");
                        }
                        index = endingIndex;
                    }
                    shouldContinue = match.Success;
                }
            }
            e.Result = count;
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

        private bool IsValidGeometry(string pathGeometry) {
            try {
                Geometry.Parse(pathGeometry);
                return true;
            } catch (Exception) {
                return false;
            }
        }
    }
}
