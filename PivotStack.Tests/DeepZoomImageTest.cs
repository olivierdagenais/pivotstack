using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using NUnit.Framework;
using SoftwareNinjas.Core;
using EnumerableExtensions = SoftwareNinjas.Core.Test.EnumerableExtensions;
using Tile = SoftwareNinjas.Core.Pair<System.Drawing.Rectangle, string>;

namespace PivotStack.Tests
{
    [TestFixture]
    public class DeepZoomImageTest
    {
        private static readonly Size PortraitImageSize = new Size (1200, 1500);
        private static readonly Size PowerOfTwoSize = new Size (1024, 512);
        private static readonly Size SquareLogoSize = new Size (700, 700);

        private static void AssertAreEqual (System.Windows.Rect expected, Rectangle actual)
        {
            Assert.AreEqual ((int) expected.X,      actual.X);
            Assert.AreEqual ((int) expected.Y,      actual.Y);
            Assert.AreEqual ((int) expected.Width,  actual.Width);
            Assert.AreEqual ((int) expected.Height, actual.Height);

            Assert.AreEqual ((int) expected.Top,    actual.Top);
            Assert.AreEqual ((int) expected.Bottom, actual.Bottom);
            Assert.AreEqual ((int) expected.Left,   actual.Left);
            Assert.AreEqual ((int) expected.Right,  actual.Right);
        }

        private static void CheckRectangleBySize(int width, int height)
        {
            var expected = new System.Windows.Rect (new System.Windows.Size (width, height));
            var actual = DeepZoomImage.CreateRectangle (new Size (width, height));
            AssertAreEqual (expected, actual);
        }

        [Test]
        public void CreateRectangle_WithSize_EdgeCases()
        {
            CheckRectangleBySize (         0,          0);
            CheckRectangleBySize (         1,          1);
            CheckRectangleBySize (       255,        255);
            CheckRectangleBySize (     32768,      32768);
            CheckRectangleBySize (2147483647, 2147483647);
        }

        private static void CheckRectangleWithTwoPoints(int x1, int y1, int x2, int y2)
        {
            // The constructor below is documented as:
            // "Initializes a new instance of the Rect structure that is
            // exactly large enough to contain the two specified points."
            var expected = new System.Windows.Rect
            (
                new System.Windows.Point (x1, y1),
                new System.Windows.Point (x2 + 1, y2 + 1)
            );
            var actual = DeepZoomImage.CreateRectangle (new Point (x1, y1), new Point (x2, y2));
            AssertAreEqual (expected, actual);
        }

        [Test]
        public void CreateRectangle_WithTwoPoints_Typical()
        {
            CheckRectangleWithTwoPoints (  0,   0, 254, 254);
            CheckRectangleWithTwoPoints (  0, 253, 254, 374);
            CheckRectangleWithTwoPoints (253,   0, 299, 254);
            CheckRectangleWithTwoPoints (253, 253, 299, 374);
        }

        [Test]
        public void CreateRectangle_WithTwoPoints_EdgeCases()
        {
            CheckRectangleWithTwoPoints (  0,   0,   0,   0);
            CheckRectangleWithTwoPoints (  0,   0,   1,   1);
            CheckRectangleWithTwoPoints (  1,   1,   1,   1);
            CheckRectangleWithTwoPoints (  2,   2,   2,   2);
            CheckRectangleWithTwoPoints (  2,   2,   4,   4);
            CheckRectangleWithTwoPoints (  0,   0, 255, 255);
            CheckRectangleWithTwoPoints (  2,   2,   2,   2);
        }

        [Test]
        public void DetermineMaximumLevel_Base ()
        {
            Assert.AreEqual (11, DeepZoomImage.DetermineMaximumLevel (new Size (1200, 1500)));
            Assert.AreEqual (10, DeepZoomImage.DetermineMaximumLevel (new Size (1024, 512)));
            Assert.AreEqual (0, DeepZoomImage.DetermineMaximumLevel (new Size (1, 1)));
        }

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
        public void ComputeLevelSize_SquareLogo ()
        {
            Assert.AreEqual (new Size (700, 700), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 11));
            Assert.AreEqual (new Size (700, 700), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 10));
            Assert.AreEqual (new Size (350, 350), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 9));
            Assert.AreEqual (new Size (175, 175), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 8));
            Assert.AreEqual (new Size (88, 88), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 7));
            Assert.AreEqual (new Size (44, 44), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 6));
            Assert.AreEqual (new Size (22, 22), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 5));
            Assert.AreEqual (new Size (11, 11), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 4));
            Assert.AreEqual (new Size (6, 6), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 3));
            Assert.AreEqual (new Size (3, 3), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 2));
            Assert.AreEqual (new Size (2, 2), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 1));
            Assert.AreEqual (new Size (1, 1), DeepZoomImage.ComputeLevelSize (SquareLogoSize, 0));
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
            var expected = new[] {new Tile (DeepZoomImage.CreateRectangle (size), "0_0")};
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_SizeOfTile ()
        {
            var size = new Size (254, 254);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[] {new Tile (DeepZoomImage.CreateRectangle (size), "0_0")};
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BiggerThanTile ()
        {
            var size = new Size (300, 375);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 253), new Point(254, 374)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(253,   0), new Point(299, 254)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(253, 253), new Point(299, 374)), "1_1"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BigWithDoubleOverlap ()
        {
            var size = new Size (700, 700);
            var actual = DeepZoomImage.ComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 253), new Point(254, 508)), "0_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 507), new Point(254, 699)), "0_2"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(253,   0), new Point(508, 254)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(253, 253), new Point(508, 508)), "1_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(253, 507), new Point(508, 699)), "1_2"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(507,   0), new Point(699, 254)), "2_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(507, 253), new Point(699, 508)), "2_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(507, 507), new Point(699, 699)), "2_2"),
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
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(255, 255)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 252), new Point(255, 374)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(252,   0), new Point(299, 255)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(252, 252), new Point(299, 374)), "1_1"),
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
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(256, 256)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 251), new Point(256, 374)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(251,   0), new Point(299, 256)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(251, 251), new Point(299, 374)), "1_1"),
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
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 253), new Point(254, 254)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(253,   0), new Point(254, 254)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(253, 253), new Point(254, 254)), "1_1"),
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
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 252), new Point(254, 254)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(252,   0), new Point(254, 254)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(252, 252), new Point(254, 254)), "1_1"),
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
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 253), new Point(254, 255)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(253,   0), new Point(255, 254)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(253, 253), new Point(255, 255)), "1_1"),
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
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(255, 255)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 252), new Point(255, 255)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(252,   0), new Point(255, 255)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(252, 252), new Point(255, 255)), "1_1"),
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
                new Tile (DeepZoomImage.CreateRectangle (new Point(   0,    0), new Point( 254,  254)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(   0,  253), new Point( 254,  508)), "0_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(   0,  507), new Point( 254,  762)), "0_2"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(   0,  761), new Point( 254, 1016)), "0_3"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(   0, 1015), new Point( 254, 1270)), "0_4"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(   0, 1269), new Point( 254, 1499)), "0_5"),

                new Tile (DeepZoomImage.CreateRectangle (new Point( 253,    0), new Point( 508,  254)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 253,  253), new Point( 508,  508)), "1_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 253,  507), new Point( 508,  762)), "1_2"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 253,  761), new Point( 508, 1016)), "1_3"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 253, 1015), new Point( 508, 1270)), "1_4"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 253, 1269), new Point( 508, 1499)), "1_5"),

                new Tile (DeepZoomImage.CreateRectangle (new Point( 507,    0), new Point( 762,  254)), "2_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 507,  253), new Point( 762,  508)), "2_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 507,  507), new Point( 762,  762)), "2_2"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 507,  761), new Point( 762, 1016)), "2_3"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 507, 1015), new Point( 762, 1270)), "2_4"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 507, 1269), new Point( 762, 1499)), "2_5"),

                new Tile (DeepZoomImage.CreateRectangle (new Point( 761,    0), new Point(1016,  254)), "3_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 761,  253), new Point(1016,  508)), "3_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 761,  507), new Point(1016,  762)), "3_2"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 761,  761), new Point(1016, 1016)), "3_3"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 761, 1015), new Point(1016, 1270)), "3_4"),
                new Tile (DeepZoomImage.CreateRectangle (new Point( 761, 1269), new Point(1016, 1499)), "3_5"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(1015,    0), new Point(1199,  254)), "4_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(1015,  253), new Point(1199,  508)), "4_1"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(1015,  507), new Point(1199,  762)), "4_2"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(1015,  761), new Point(1199, 1016)), "4_3"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(1015, 1015), new Point(1199, 1270)), "4_4"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(1015, 1269), new Point(1199, 1499)), "4_5"),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_AllLevelsOfSquareLogo ()
        {
            var tester = new Action<int, int, int> ((inputSide, expectedTileCount, expectedFirstTileSize) =>
                {
                    var tiles = DeepZoomImage.ComputeTiles (new Size (inputSide, inputSide), 254, 1);
                    var e = tiles.GetEnumerator ();
                    e.MoveNext ();
                    var firstTile = e.Current;
                    var actualRectangle = firstTile.First;
                    var expectedRectangle = new Rectangle (0, 0, expectedFirstTileSize, expectedFirstTileSize);
                    Assert.AreEqual (expectedRectangle, actualRectangle);
                    var actualTileCount = 1;
                    while (e.MoveNext ())
                    {
                        actualTileCount++;
                    }
                    Assert.AreEqual (expectedTileCount, actualTileCount);
                }
            );

            tester (700, 9, 255);
            tester (350, 4, 255);
            tester (175, 1, 175);
            tester (88, 1, 88);
            tester (44, 1, 44);
            tester (22, 1, 22);
            tester (11, 1, 11);
            tester (6, 1, 6);
            tester (3, 1, 3);
            tester (2, 1, 2);
            tester (1, 1, 1);
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

        [Test]
        public void Slice_Typical ()
        {
            var tiles = new[]
            {
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0,   0), new Point(254, 254)), "0_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(  0, 253), new Point(254, 374)), "0_1"),

                new Tile (DeepZoomImage.CreateRectangle (new Point(253,   0), new Point(299, 254)), "1_0"),
                new Tile (DeepZoomImage.CreateRectangle (new Point(253, 253), new Point(299, 374)), "1_1"),
            };
            var streams = new Dictionary<string, MemoryStream>
            {
                {"0_0", new MemoryStream()},
                {"0_1", new MemoryStream()},
                {"1_0", new MemoryStream()},
                {"1_1", new MemoryStream()},
            };

            try
            {
                using (var inputStream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomImageTest> ("300x375.png"))
                using (var sourceBitmap = new Bitmap (inputStream))
                {
                    DeepZoomImage.Slice (sourceBitmap, tiles, ImageFormat.Png, tilename => streams[tilename]);
                }

                foreach (var keyValuePair in streams)
                {
                    var expectedResourceFileName = keyValuePair.Key + ".png";
                    var actualStream = keyValuePair.Value;
                    ProgramTest.AssertStreamsAreEqual<DeepZoomImageTest> (expectedResourceFileName, actualStream);
                }

            }
            finally
            {
                foreach (var stream in streams.Values)
                {
                    stream.Close ();
                    stream.Dispose ();
                }
            }
        }

        [Test]
        public void Slice_SquareLogo()
        {
            Assert.AreEqual (10, DeepZoomImage.DetermineMaximumLevel (SquareLogoSize));
            using (var sourceBitmap = new Bitmap (SquareLogoSize.Width, SquareLogoSize.Height))
            {
                var tester = new Action<int, IEnumerable<Size>> ((level, expectedSliceSizes) =>
                    {
                        var levelSize = DeepZoomImage.ComputeLevelSize (SquareLogoSize, level);
                        var tiles = DeepZoomImage.ComputeTiles (levelSize, 254, 1);
                        var slices = DeepZoomImage.Slice (sourceBitmap, tiles);
                        var actualSliceSizes = slices.Map (pair => pair.First.Size);
                        EnumerableExtensions.EnumerateSame (expectedSliceSizes, actualSliceSizes);
                    }
                );

                tester (10, new[]
                    {
                        new Size(255, 255), new Size(255, 256), new Size(255, 193),
                        new Size(256, 255), new Size(256, 256), new Size(256, 193),
                        new Size(193, 255), new Size(193, 256), new Size(193, 193),  
                    }
                );
                tester (9, new[]
                    {
                        new Size(255, 255), new Size(255, 97),
                        new Size(97, 255), new Size(97, 97),  
                    }
                );
                tester (8, new[] {new Size (175, 175)});
                tester (7, new[] {new Size (88, 88)});
                tester (6, new[] {new Size (44, 44)});
                tester (5, new[] {new Size (22, 22)});
                tester (4, new[] {new Size (11, 11)});
                tester (3, new[] {new Size (6, 6)});
                tester (2, new[] {new Size (3, 3)});
                tester (1, new[] {new Size (2, 2)});
                tester (0, new[] {new Size (1, 1)});
            }
        }
    }
}
