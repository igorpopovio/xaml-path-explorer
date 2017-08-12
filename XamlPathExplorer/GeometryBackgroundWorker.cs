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
            var files = LoadAllFilesFrom(e.Argument as string[]);

            foreach (var file in files) {
                var fileContents = file.OpenText().ReadToEnd();

                var index = 0;
                var shouldContinue = true;

                while (shouldContinue) {
                    var match = PathGeometryRegex.Match(fileContents, index);
                    if (match.Success) {
                        var pathDetails = new PathDetails();
                        pathDetails.File = file;
                        pathDetails.StartingIndex = FindBackwardDelimiters(file, fileContents, match.Index, Delimiters) + 1;
                        pathDetails.EndingIndex = FindForwardDelimiters(file, fileContents, match.Index, Delimiters);
                        pathDetails.Length = pathDetails.EndingIndex - pathDetails.StartingIndex;
                        pathDetails.LineNumber = fileContents.Take(pathDetails.StartingIndex).Count(c => c == '\n') + 1;
                        pathDetails.Geometry = fileContents.Substring(pathDetails.StartingIndex, pathDetails.Length);
                        pathDetails.Geometry = HttpUtility.HtmlDecode(pathDetails.Geometry);

                        if (IsValidGeometry(pathDetails.Geometry)) {
                            // the sleep adds a nice effect when adding items
                            // we need to have this only for a few items that fit in the current view
                            // if we have too many then the loading will be too slow
                            if (count < 100) Thread.Sleep(10);

                            // FIXME: add progress bar and report proper percentage
                            ReportProgress(0, pathDetails);
                            count++;
                        } else {
                            // FIXME: you can't update the UI from background worker threads
                            // use https://stackoverflow.com/questions/1044460/unhandled-exceptions-in-backgroundworker
                            // System.Windows.Forms.MessageBox.Show($"Failed to render geometry: {pathGeometry}\n from\n{file.FullName}!");
                        }
                        index = pathDetails.EndingIndex;
                    }
                    shouldContinue = match.Success;
                }
            }
            e.Result = count;
        }

        private static IEnumerable<FileInfo> LoadAllFilesFrom(string[] filesArgument) {
            var files = new List<FileInfo>();
            foreach (var file in filesArgument) {
                if (File.Exists(file)) {
                    var fileInfo = new FileInfo(file);
                    switch (fileInfo.Extension) {
                        case ".xaml":
                            files.Add(fileInfo);
                            break;
                        case ".csproj":
                        case ".sln":
                            files.AddRange(LoadAllFilesFrom(new string[] { fileInfo.Directory.FullName }));
                            break;
                    }
                } else if (Directory.Exists(file)) {
                    var directoryInfo = new DirectoryInfo(file);
                    files.AddRange(directoryInfo.GetFiles("*.xaml", SearchOption.AllDirectories));
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
