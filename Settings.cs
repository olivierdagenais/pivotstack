using System;
using System.Drawing.Imaging;

namespace PivotStack
{
    public struct Settings
    {
        public string SiteDomain { get; set; }
        public int MaximumNumberOfItems { get; set; }
        public ImageFormat PostImageEncoding { get; set; }
        // TODO: path to XAML template?
        public string PathToFavIcon { get; set; }
        public string PathToBrandImage { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string PathToOutput { get; set; }
        // TODO: DeepZoom collection parameters?
        // TODO: Number of threads to use?

        public int MaximumNumberOfDecimalPlaces
        {
            get
            {
                return (int) Math.Log10 (MaximumNumberOfItems);
            }
        }

        public string FileNameIdFormat
        {
            get
            {
                return new String ('0', MaximumNumberOfDecimalPlaces);
            }
        }
    }
}
