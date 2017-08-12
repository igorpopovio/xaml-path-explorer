﻿using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace XamlPathExplorer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            textEditor.Options.EnableHyperlinks = false;


            LoadXamlSyntaxHighlightingDefinition();

            var resourceNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var xamlSyntax = "XamlPathExplorer.Resources.studiostyles.xaml";
            using (var stream = Application.ResourceAssembly.GetManifestResourceStream(xamlSyntax)) {
                textEditor.Load(stream);
            }
        }

        private void LoadXamlSyntaxHighlightingDefinition() {
            var xamlSyntax = "XamlPathExplorer.Resources.xaml-syntax.xshd";
            using (var stream = Application.ResourceAssembly.GetManifestResourceStream(xamlSyntax)) {
                using (var reader = new XmlTextReader(stream)) {
                    textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        public void wrapPanel_DragEnter(object sender, DragEventArgs e) {
            dragAndDropContainer.Style = (Style)FindResource("DragEnter");
        }

        public void wrapPanel_DragLeave(object sender, DragEventArgs e) {
            dragAndDropContainer.Style = (Style)FindResource("DragLeave");
        }

        public void wrapPanel_Drop(object sender, DragEventArgs e) {
            dragAndDropContainer.Style = (Style)FindResource("DragLeave");

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadGeometryInBackgroundThreadFrom(files);
            }
        }

        public void LoadGeometryInBackgroundThreadFrom(string[] files) {
            var worker = new GeometryBackgroundWorker();
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            itemsPanel.Children.Clear();
            worker.RunWorkerAsync(files);
            statusBarItem.Content = $"Starting to load paths from {files}";
        }

        public void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            var count = e.Result as int?;
            statusBarItem.Content = $"Loaded {count} paths";
        }

        public void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            var pathDetails = e.UserState as PathDetails;
            LoadGeometryFrom(pathDetails);
        }

        public void LoadGeometryFrom(PathDetails pathDetails) {
            var pathButton = new PathButton { PathDetails = pathDetails };
            pathButton.Editor = textEditor;
            pathButton.LoadedFileLabel = LoadedFile;
            itemsPanel.Children.Add(pathButton);
        }
    }
}
