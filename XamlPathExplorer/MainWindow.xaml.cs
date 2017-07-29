﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace XamlPathExplorer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e) {
            var defaultDirectory = @"C:\Projects\napa\monitor-team\loading-computer\Client";
            // LoadDummyPaths();
            LoadPathsFrom(defaultDirectory);
        }

        private void LoadPathsFrom(string defaultDirectory) {
            var numberRegex = @"[-+]?[0-9]*\.?[0-9]+(?:[eE][-+]?[0-9]+)?";
            var pathGeometryRegex = $@"[MLCV]\s*{numberRegex}(?:[,\s]{numberRegex})*";
            var regex = new Regex(pathGeometryRegex, RegexOptions.IgnoreCase & RegexOptions.Multiline & RegexOptions.Compiled);

            
            var directoryInfo = new DirectoryInfo(defaultDirectory);
            var files = "";
            foreach (var file in directoryInfo.GetFiles("*.xaml", SearchOption.AllDirectories)) {
                var match = regex.Match(file.OpenText().ReadToEnd(), 0);
                if (match.Success) System.Windows.Forms.MessageBox.Show("Found in "  + file.FullName + ":\n"  + match.Value + " at position: " + match.Index);
                files += file.FullName + "\n";
            }
        }

        private void LoadDummyPaths() {
            var defaultGeometry = "M9.5999776,12.699997L11.899997,15.099998 13.299991,14.300003 19.000005,20 22.899999,18 28.299995,20.699997 28.299995,25.699997 3.5999767,25.699997 3.5999767,18.599998z M20.500005,11.699997L21.199987,11.699997 21.000005,12.800003 20.699987,12.800003z M23.09998,10.699997L23.799993,11.599998 23.59998,11.800003 22.699987,11.099998z M18.699987,10.699997L19.199987,11.199997 18.299993,11.900002 18.000005,11.599998z M23.59998,8.5999985L24.699987,8.8000031 24.699987,9 23.59998,9.1999969z M18.09998,8.5999985L18.09998,9.3000031 17.000005,9 17.000005,8.8000031z M20.799993,7C21.899999,7 22.699987,7.9000015 22.699987,8.9000015 22.699987,10 21.799993,10.800003 20.799993,10.800003 19.699987,10.800003 18.899999,9.9000015 18.899999,8.9000015 18.899999,7.8000031 19.799993,7 20.799993,7z M23.500005,6.0999985L23.699987,6.3000031 23.000005,7.1999969 22.500005,6.6999969z M18.199987,6.0999985L19.09998,6.8000031 18.59998,7.3000031 18.000005,6.3000031z M20.699987,5L21.000005,5 21.199987,6.0999985 20.500005,6.0999985z M2.1999823,2.4000015L2.1999823,26.800003 29.400001,26.800003 29.400001,2.4000015z M0,0L31.900001,0 31.900001,32 0,32z";

            for (int i = 0; i < 10; i++)
                wrapPanel.Children.Add(new PathButton { PathGeometry = Geometry.Parse(defaultGeometry) });
        }
    }
}
