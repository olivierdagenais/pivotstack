using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;

namespace PivotStack
{
    public class Settings
    {
        public string SiteDomain { get; set; }
        public int MaximumNumberOfItems { get; set; }
        public int HighestId { get; set; }
        public ImageFormat PostImageEncoding { get; set; }
        // TODO: path to XAML template?
        public string PathToFavIcon { get; set; }
        public string PathToBrandImage { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string AbsoluteWorkingFolder { get; set; }
        public string AbsoluteOutputFolder { get; set; }
        public Size ItemImageSize { get; set; }
        // TODO: Do we really need to be able to parameterize TileSize & TileOverlap?
        public int TileSize { get; set; }
        public int TileOverlap { get; set; }
        // TODO: DeepZoom collection parameters?
        // TODO: Number of threads to use?
        public XmlReaderSettings XmlReaderSettings { get; set; }
        public XmlWriterSettings XmlWriterSettings { get; set; }

        public int MaximumNumberOfDigits
        {
            get
            {
                return 1 + (int) Math.Log10 (HighestId);
            }
        }

        public string FileNameIdFormat
        {
            get
            {
                return new String ('0', MaximumNumberOfDigits);
            }
        }
    }
}
