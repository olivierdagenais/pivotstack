using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PivotStack
{
    public static class FrameworkElementExtensions
    {
        /// <remarks>
        /// Stolen and adapted from
        /// <see href="http://www.vistax64.com/avalon/180411-save-wpf-control-png-image.html">
        ///     Save WPF Control as PNG image
        /// </see>
        /// </remarks>
        internal static BitmapSource ToBitmapSource(this FrameworkElement obj)
        {
            if (Equals(obj.Width, Double.NaN))
            {
                throw new ArgumentOutOfRangeException("obj", "Object width was not specified");
            }

            if (Equals(obj.Height, Double.NaN))
            {
                throw new ArgumentOutOfRangeException ("obj", "Object height was not specified");
            }

            // Save current canvas transform
            var transform = obj.LayoutTransform;
            obj.LayoutTransform = null;
            
            // fix margin offset as well
            var margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            var size = new Size(obj.Width, obj.Height);
            
            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            var bmp = new RenderTargetBitmap((int) obj.Width, (int) obj.Height, 96, 96, PixelFormats.Pbgra32);
            
            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }
    }
}
