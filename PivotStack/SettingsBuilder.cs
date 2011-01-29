using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;

namespace PivotStack
{
    public class SettingsBuilder
    {
        public string SiteDomain { get; set; }
        public int MaximumNumberOfItems { get; set; }
        public int HighestId { get; set; }
        public ImageFormat PostImageEncoding { get; set; }
        public string PathToFavIcon { get; set; }
        public string PathToBrandImage { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string AbsoluteWorkingFolder { get; set; }
        public string AbsoluteOutputFolder { get; set; }
        public Size ItemImageSize { get; set; }
        public int TileSize { get; set; }
        public int TileOverlap { get; set; }
        public XmlReaderSettings XmlReaderSettings { get; set; }
        public XmlWriterSettings XmlWriterSettings { get; set; }
    }
}
