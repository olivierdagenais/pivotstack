using GdiPlus = System.Drawing;
using Wpf = System.Windows.Media;
using System.Windows;

namespace PivotStack
{
    public static class BitmapSourceExtensions
    {
        /// <remarks>
        /// Stolen and adapted from
        /// <seealso href="http://stackoverflow.com/questions/2284353/is-there-a-good-way-to-convert-between-bitmapsource-and-bitmap">
        ///     Is there a good way to convert between BitmapSource and Bitmap?
        /// </seealso>
        /// </remarks>
        internal static GdiPlus.Bitmap ConvertToGdiPlusBitmap (this Wpf.Imaging.BitmapSource bitmapSource)
        {
            var bmp = new GdiPlus.Bitmap (
                bitmapSource.PixelWidth,
                bitmapSource.PixelHeight,
                GdiPlus.Imaging.PixelFormat.Format32bppPArgb);

            var rectangle = new GdiPlus.Rectangle (GdiPlus.Point.Empty, bmp.Size);
            var data = bmp.LockBits (
                rectangle,
                GdiPlus.Imaging.ImageLockMode.WriteOnly,
                GdiPlus.Imaging.PixelFormat.Format32bppPArgb
                );
            bitmapSource.CopyPixels (
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits (data);
            return bmp;
        }
    }
}
