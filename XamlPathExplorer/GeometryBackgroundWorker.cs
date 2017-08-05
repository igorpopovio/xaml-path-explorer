using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Media;

namespace XamlPathExplorer {
    public class GeometryBackgroundWorker : BackgroundWorker {
        private const string Delimiters = "\"<>";
        private const string PathGeometryRegexString =
            @"[MLCV]\s*[-+]?[0-9]*\.?[0-9]+(?:[eE][-+]?[0-9]+)?[,\s]+";
        private readonly Regex PathGeometryRegex =
            new Regex(PathGeometryRegexString,
                RegexOptions.IgnoreCase & RegexOptions.Multiline & RegexOptions.Compiled);

        private int count;

        public GeometryBackgroundWorker() {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
            DoWork += Worker_DoWork;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e) {
            var files = LoadAllFilesFrom(e.Argument as string[], ".xaml");

            foreach (var file in files) {
                var fileContents = file.OpenText().ReadToEnd();

                var index = 0;
                var shouldContinue = true;

                while (shouldContinue) {
                    var match = PathGeometryRegex.Match(fileContents, index);
                    if (match.Success) {
                        var startingIndex = FindBackwardDelimiters(file, fileContents, match.Index, Delimiters) + 1;
                        var endingIndex = FindForwardDelimiters(file, fileContents, match.Index, Delimiters);
                        var pathGeometry = fileContents.Substring(startingIndex, endingIndex - startingIndex);
                        pathGeometry = HttpUtility.HtmlDecode(pathGeometry);
                        if (IsValidGeometry(pathGeometry)) {
                            // Thread.Sleep(10);
                            ReportProgress(0, pathGeometry);
                            count++;
                        } else {
                            // FIXME: you can't update the UI from background worker threads
                            // use https://stackoverflow.com/questions/1044460/unhandled-exceptions-in-backgroundworker
                            // System.Windows.Forms.MessageBox.Show($"Failed to render geometry: {pathGeometry}\n from\n{file.FullName}!");
                        }
                        index = endingIndex;
                    }
                    shouldContinue = match.Success;
                }
            }
            e.Result = count;
        }

        private static IEnumerable<FileInfo> LoadAllFilesFrom(string[] filesArgument, string extension) {
            var files = new List<FileInfo>();
            foreach (var file in filesArgument) {
                if (File.Exists(file)) {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Extension == extension)
                        files.Add(fileInfo);
                } else if (Directory.Exists(file)) {
                    var directoryInfo = new DirectoryInfo(file);
                    files.AddRange(directoryInfo.GetFiles($"*{extension}", SearchOption.AllDirectories));
                } else {
                    throw new Exception($"Cannot load path: {file}");
                }
            }
            return files;
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
