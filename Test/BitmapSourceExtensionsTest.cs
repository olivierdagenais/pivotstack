using System.IO;
using GdiPlus = System.Drawing;
using Wpf = System.Windows.Media;

using NUnit.Framework;
using SoftwareNinjas.Core;

namespace PivotStack.Test
{
    [TestFixture]
    public class BitmapSourceExtensionsTest
    {
        [Test]
        public void ConvertToGdiPlusBitmap()
        {
            // arrange
            Wpf.Imaging.BitmapSource bitmapSource;
            const string fileName = "1200x1500.png";
            using (var inputStream = AssemblyExtensions.OpenScopedResourceStream<BitmapSourceExtensionsTest> (fileName))
            {
                var decoder = new Wpf.Imaging.PngBitmapDecoder (
                    inputStream,
                    Wpf.Imaging.BitmapCreateOptions.PreservePixelFormat,
                    Wpf.Imaging.BitmapCacheOption.Default
                    );
                bitmapSource = decoder.Frames[0];
            }

            // act
            var bitmap = bitmapSource.ConvertToGdiPlusBitmap ();

            // assert
            using (var actualStream = new MemoryStream())
            {
                bitmap.Save (actualStream, GdiPlus.Imaging.ImageFormat.Png);
                ProgramTest.AssertStreamsAreEqual<BitmapSourceExtensionsTest> (fileName, actualStream);
            }
        }
    }
}
