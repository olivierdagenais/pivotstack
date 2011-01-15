using System.Drawing.Imaging;

namespace PivotStack
{
    public static class ImageFormatExtensions
    {
        public static string GetName(this ImageFormat imageFormat)
        {
            return null == imageFormat
                ? null
                : imageFormat.ToString ().ToLower ();
        }
    }
}
