using NUnit.Framework;

namespace PivotStack.Test
{
    [TestFixture]
    public class DeepZoomImageTest
    {
        private static readonly Size<int> PortraitImageSize = Size.Create (1200, 1500);
        private static readonly Size<int> PowerOfTwoSize = Size.Create (1024, 512);

        [Test]
        public void ComputeLevelSize_Base ()
        {
            Assert.AreEqual (Size.Create (1, 1), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 0));
        }

        [Test]
        public void ComputeLevelSize_Typical ()
        {
            Assert.AreEqual (Size.Create (1200, 1500), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 12));
            Assert.AreEqual (Size.Create (1200, 1500), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 11));
            Assert.AreEqual (Size.Create (600, 750), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 10));
            Assert.AreEqual (Size.Create (300, 375), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 9));
            Assert.AreEqual (Size.Create (150, 188), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 8));
            Assert.AreEqual (Size.Create (75, 94), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 7));
            Assert.AreEqual (Size.Create (38, 47), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 6));
            Assert.AreEqual (Size.Create (19, 24), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 5));
            Assert.AreEqual (Size.Create (10, 12), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 4));
            Assert.AreEqual (Size.Create (5, 6), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 3));
            Assert.AreEqual (Size.Create (3, 3), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 2));
            Assert.AreEqual (Size.Create (2, 2), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 1));
            Assert.AreEqual (Size.Create (1, 1), DeepZoomImage.ComputeLevelSize (PortraitImageSize, 0));
        }

        [Test]
        public void ComputeLevelSize_PowerOfTwo ()
        {
            Assert.AreEqual (Size.Create (1024, 512), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 11));
            Assert.AreEqual (Size.Create (1024, 512), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 10));
            Assert.AreEqual (Size.Create (512, 256), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 9));
            Assert.AreEqual (Size.Create (256, 128), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 8));
            Assert.AreEqual (Size.Create (128, 64), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 7));
            Assert.AreEqual (Size.Create (64, 32), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 6));
            Assert.AreEqual (Size.Create (32, 16), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 5));
            Assert.AreEqual (Size.Create (16, 8), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 4));
            Assert.AreEqual (Size.Create (8, 4), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 3));
            Assert.AreEqual (Size.Create (4, 2), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 2));
            Assert.AreEqual (Size.Create (2, 1), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 1));
            Assert.AreEqual (Size.Create (1, 1), DeepZoomImage.ComputeLevelSize (PowerOfTwoSize, 0));
        }

    }
}
