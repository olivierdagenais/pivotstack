using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

using NUnit.Framework;
using SoftwareNinjas.Core;
using EnumerableExtensions = SoftwareNinjas.Core.Test.EnumerableExtensions;

namespace PivotStack.Test
{
    [TestFixture]
    public class DeepZoomImageTest
    {
        private static readonly Size PortraitImageSize = new Size (1200, 1500);
        private static readonly Size PowerOfTwoSize = new Size (1024, 512);

        [Test]
        public void ComputeLevelSize_Base ()
        {
            Assert.AreEqual (new Size (1, 1), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 0));
        }

        [Test]
        public void ComputeLevelSize_Typical ()
        {
            Assert.AreEqual (new Size (1200, 1500), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 12));
            Assert.AreEqual (new Size (1200, 1500), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 11));
            Assert.AreEqual (new Size (600, 750), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 10));
            Assert.AreEqual (new Size (300, 375), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 9));
            Assert.AreEqual (new Size (150, 188), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 8));
            Assert.AreEqual (new Size (75, 94), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 7));
            Assert.AreEqual (new Size (38, 47), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 6));
            Assert.AreEqual (new Size (19, 24), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 5));
            Assert.AreEqual (new Size (10, 12), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 4));
            Assert.AreEqual (new Size (5, 6), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 3));
            Assert.AreEqual (new Size (3, 3), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 2));
            Assert.AreEqual (new Size (2, 2), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 1));
            Assert.AreEqual (new Size (1, 1), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 0));
        }

        [Test]
        public void ComputeLevelSize_PowerOfTwo ()
        {
            Assert.AreEqual (new Size (1024, 512), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 11));
            Assert.AreEqual (new Size (1024, 512), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 10));
            Assert.AreEqual (new Size (512, 256), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 9));
            Assert.AreEqual (new Size (256, 128), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 8));
            Assert.AreEqual (new Size (128, 64), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 7));
            Assert.AreEqual (new Size (64, 32), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 6));
            Assert.AreEqual (new Size (32, 16), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 5));
            Assert.AreEqual (new Size (16, 8), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 4));
            Assert.AreEqual (new Size (8, 4), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 3));
            Assert.AreEqual (new Size (4, 2), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 2));
            Assert.AreEqual (new Size (2, 1), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 1));
            Assert.AreEqual (new Size (1, 1), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 0));
        }

        [Test]
        public void ComputeTiles_SmallerThanTile ()
        {
            var size = new Size(150, 188);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[] {new Pair<Rect, string> (new Rect (size), "0_0")};
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_SizeOfTile ()
        {
            var size = new Size (254, 254);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[] {new Pair<Rect, string> (new Rect (size), "0_0")};
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BiggerThanTile ()
        {
            var size = new Size (300, 375);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(  0, 253), new Point(254, 374)), "0_1"),

                new Pair<Rect, string> (new Rect (new Point(253,   0), new Point(299, 254)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(253, 253), new Point(299, 374)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BiggerThanTileWithOverlapOfTwo ()
        {
            var size = new Size (300, 375);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 2);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(  0,   0), new Point(255, 255)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(  0, 252), new Point(255, 374)), "0_1"),

                new Pair<Rect, string> (new Rect (new Point(252,   0), new Point(299, 255)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(252, 252), new Point(299, 374)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BiggerThanTileWithOverlapOfThree ()
        {
            var size = new Size (300, 375);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 3);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(  0,   0), new Point(256, 256)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(  0, 251), new Point(256, 374)), "0_1"),

                new Pair<Rect, string> (new Rect (new Point(251,   0), new Point(299, 256)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(251, 251), new Point(299, 374)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_OnePixelBiggerThanTile ()
        {
            var size = new Size (255, 255);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(  0, 253), new Point(254, 254)), "0_1"),

                new Pair<Rect, string> (new Rect (new Point(253,   0), new Point(254, 254)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(253, 253), new Point(254, 254)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_OnePixelBiggerThanTileWithOverlapOfTwo ()
        {
            var size = new Size (255, 255);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 2);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(  0, 252), new Point(254, 254)), "0_1"),

                new Pair<Rect, string> (new Rect (new Point(252,   0), new Point(254, 254)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(252, 252), new Point(254, 254)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_TwoPixelsBiggerThanTile ()
        {
            var size = new Size (256, 256);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(  0, 253), new Point(254, 255)), "0_1"),

                new Pair<Rect, string> (new Rect (new Point(253,   0), new Point(255, 254)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(253, 253), new Point(255, 255)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_TwoPixelsBiggerThanTileWithOverlapOfTwo ()
        {
            var size = new Size (256, 256);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 2);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(  0,   0), new Point(255, 255)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(  0, 252), new Point(255, 255)), "0_1"),

                new Pair<Rect, string> (new Rect (new Point(252,   0), new Point(255, 255)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(252, 252), new Point(255, 255)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_OriginalSize ()
        {
            var size = new Size (1200, 1500);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Pair<Rect, string> (new Rect (new Point(   0,    0), new Point( 254,  254)), "0_0"),
                new Pair<Rect, string> (new Rect (new Point(   0,  253), new Point( 254,  508)), "0_1"),
                new Pair<Rect, string> (new Rect (new Point(   0,  507), new Point( 254,  762)), "0_2"),
                new Pair<Rect, string> (new Rect (new Point(   0,  761), new Point( 254, 1016)), "0_3"),
                new Pair<Rect, string> (new Rect (new Point(   0, 1015), new Point( 254, 1270)), "0_4"),
                new Pair<Rect, string> (new Rect (new Point(   0, 1269), new Point( 254, 1499)), "0_5"),

                new Pair<Rect, string> (new Rect (new Point( 253,    0), new Point( 508,  254)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point( 253,  253), new Point( 508,  508)), "1_1"),
                new Pair<Rect, string> (new Rect (new Point( 253,  507), new Point( 508,  762)), "1_2"),
                new Pair<Rect, string> (new Rect (new Point( 253,  761), new Point( 508, 1016)), "1_3"),
                new Pair<Rect, string> (new Rect (new Point( 253, 1015), new Point( 508, 1270)), "1_4"),
                new Pair<Rect, string> (new Rect (new Point( 253, 1269), new Point( 508, 1499)), "1_5"),

                new Pair<Rect, string> (new Rect (new Point( 507,    0), new Point( 762,  254)), "2_0"),
                new Pair<Rect, string> (new Rect (new Point( 507,  253), new Point( 762,  508)), "2_1"),
                new Pair<Rect, string> (new Rect (new Point( 507,  507), new Point( 762,  762)), "2_2"),
                new Pair<Rect, string> (new Rect (new Point( 507,  761), new Point( 762, 1016)), "2_3"),
                new Pair<Rect, string> (new Rect (new Point( 507, 1015), new Point( 762, 1270)), "2_4"),
                new Pair<Rect, string> (new Rect (new Point( 507, 1269), new Point( 762, 1499)), "2_5"),

                new Pair<Rect, string> (new Rect (new Point( 761,    0), new Point(1016,  254)), "3_0"),
                new Pair<Rect, string> (new Rect (new Point( 761,  253), new Point(1016,  508)), "3_1"),
                new Pair<Rect, string> (new Rect (new Point( 761,  507), new Point(1016,  762)), "3_2"),
                new Pair<Rect, string> (new Rect (new Point( 761,  761), new Point(1016, 1016)), "3_3"),
                new Pair<Rect, string> (new Rect (new Point( 761, 1015), new Point(1016, 1270)), "3_4"),
                new Pair<Rect, string> (new Rect (new Point( 761, 1269), new Point(1016, 1499)), "3_5"),

                new Pair<Rect, string> (new Rect (new Point(1015,    0), new Point(1199,  254)), "4_0"),
                new Pair<Rect, string> (new Rect (new Point(1015,  253), new Point(1199,  508)), "4_1"),
                new Pair<Rect, string> (new Rect (new Point(1015,  507), new Point(1199,  762)), "4_2"),
                new Pair<Rect, string> (new Rect (new Point(1015,  761), new Point(1199, 1016)), "4_3"),
                new Pair<Rect, string> (new Rect (new Point(1015, 1015), new Point(1199, 1270)), "4_4"),
                new Pair<Rect, string> (new Rect (new Point(1015, 1269), new Point(1199, 1499)), "4_5"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void Resize_Half()
        {
            using (var inputStream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomImageTest> ("1200x1500.png"))
            using (var sourceBitmap = new Bitmap (inputStream))
            using (var targetBitmap = DeepZoomImage.Resize (sourceBitmap, 600, 750))
            using (var actualStream = new MemoryStream())
            {
                targetBitmap.Save (actualStream, ImageFormat.Png);

                ProgramTest.AssertStreamsAreEqual<DeepZoomImageTest> ("600x750.png", actualStream);
            }
        }
    }
}
