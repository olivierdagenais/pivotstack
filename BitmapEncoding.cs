using System;
using System.Windows.Media.Imaging;

namespace PivotStack
{
    public class BitmapEncoding
    {
        public static BitmapEncoding Png = new BitmapEncoding (() => new PngBitmapEncoder(), ".png");
        public static BitmapEncoding Jpeg = new BitmapEncoding (() => new JpegBitmapEncoder(), ".jpg");

        private readonly Func<BitmapEncoder> _bitmapEncoderFactory;
        private readonly string _extension;

        private BitmapEncoding (Func<BitmapEncoder> bitmapEncoderFactory, string extension)
        {
            _bitmapEncoderFactory = bitmapEncoderFactory;
            _extension = extension;
        }

        public BitmapEncoder CreateEncoder (BitmapSource bitmapSource)
        {
            var encoder = _bitmapEncoderFactory ();
            var frame = BitmapFrame.Create (bitmapSource);
            encoder.Frames.Add (frame);
            return encoder;
        }

        public string Extension
        {
            get
            {
                return _extension;
            }
        }
    }
}
