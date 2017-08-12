using System.IO;

namespace XamlPathExplorer {
    public class PathDetails {
        public string Geometry { get; set; }
        public FileInfo File { get; set; }
        public int StartingIndex { get; set; }
        public int EndingIndex { get; set; }
        public int Length { get; set; }
    }
}
