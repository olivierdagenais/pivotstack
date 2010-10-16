using System.Windows;
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
                new Pair<Rect, string> (new Rect (new Point(  0, 253), new Point(254, 375)), "0_1"),
                new Pair<Rect, string> (new Rect (new Point(253,   0), new Point(300, 254)), "1_0"),
                new Pair<Rect, string> (new Rect (new Point(253, 253), new Point(300, 375)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

    }
}
