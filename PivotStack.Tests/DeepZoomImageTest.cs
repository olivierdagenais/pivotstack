using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using NUnit.Framework;
using SoftwareNinjas.Core;
using EnumerableExtensions = SoftwareNinjas.Core.Test.EnumerableExtensions;

namespace PivotStack.Tests
{
    [TestFixture]
    public class DeepZoomImageTest
    {
        private static readonly Size PortraitImageSize = new Size (1200, 1500);
        private static readonly Size PowerOfTwoSize = new Size (1024, 512);
        private static readonly Size SquareLogoSize = new Size (700, 700);

        [Test]
        public void DetermineMaximumLevel_Base ()
        {
            Assert.AreEqual (11, DeepZoomImage.DetermineMaximumLevel (new Size (1200, 1500)));
            Assert.AreEqual (10, DeepZoomImage.DetermineMaximumLevel (new Size (1024, 512)));
            Assert.AreEqual (0, DeepZoomImage.DetermineMaximumLevel (new Size (1, 1)));
        }

        private static Size TestComputeLevelSize(Size itemImageSize, int levelNumber)
        {
            var settings = new Settings(new SettingsBuilder
            {
                ItemImageSize = itemImageSize,
            });
            var dzi = new DeepZoomImage (settings);
            return dzi.ComputeLevelSize (levelNumber);
        }

        [Test]
        public void ComputeLevelSize_Base ()
        {
            Assert.AreEqual (new Size (1, 1), TestComputeLevelSize (PortraitImageSize, 0));
        }

        [Test]
        public void ComputeLevelSize_Typical ()
        {
            Assert.AreEqual (new Size (1200, 1500), TestComputeLevelSize (PortraitImageSize, 12));
            Assert.AreEqual (new Size (1200, 1500), TestComputeLevelSize (PortraitImageSize, 11));
            Assert.AreEqual (new Size (600, 750), TestComputeLevelSize (PortraitImageSize, 10));
            Assert.AreEqual (new Size (300, 375), TestComputeLevelSize (PortraitImageSize, 9));
            Assert.AreEqual (new Size (150, 188), TestComputeLevelSize (PortraitImageSize, 8));
            Assert.AreEqual (new Size (75, 94), TestComputeLevelSize (PortraitImageSize, 7));
            Assert.AreEqual (new Size (38, 47), TestComputeLevelSize (PortraitImageSize, 6));
            Assert.AreEqual (new Size (19, 24), TestComputeLevelSize (PortraitImageSize, 5));
            Assert.AreEqual (new Size (10, 12), TestComputeLevelSize (PortraitImageSize, 4));
            Assert.AreEqual (new Size (5, 6), TestComputeLevelSize (PortraitImageSize, 3));
            Assert.AreEqual (new Size (3, 3), TestComputeLevelSize (PortraitImageSize, 2));
            Assert.AreEqual (new Size (2, 2), TestComputeLevelSize (PortraitImageSize, 1));
            Assert.AreEqual (new Size (1, 1), TestComputeLevelSize (PortraitImageSize, 0));
        }

        [Test]
        public void ComputeLevelSize_SquareLogo ()
        {
            Assert.AreEqual (new Size (700, 700), TestComputeLevelSize (SquareLogoSize, 11));
            Assert.AreEqual (new Size (700, 700), TestComputeLevelSize (SquareLogoSize, 10));
            Assert.AreEqual (new Size (350, 350), TestComputeLevelSize (SquareLogoSize, 9));
            Assert.AreEqual (new Size (175, 175), TestComputeLevelSize (SquareLogoSize, 8));
            Assert.AreEqual (new Size (88, 88), TestComputeLevelSize (SquareLogoSize, 7));
            Assert.AreEqual (new Size (44, 44), TestComputeLevelSize (SquareLogoSize, 6));
            Assert.AreEqual (new Size (22, 22), TestComputeLevelSize (SquareLogoSize, 5));
            Assert.AreEqual (new Size (11, 11), TestComputeLevelSize (SquareLogoSize, 4));
            Assert.AreEqual (new Size (6, 6), TestComputeLevelSize (SquareLogoSize, 3));
            Assert.AreEqual (new Size (3, 3), TestComputeLevelSize (SquareLogoSize, 2));
            Assert.AreEqual (new Size (2, 2), TestComputeLevelSize (SquareLogoSize, 1));
            Assert.AreEqual (new Size (1, 1), TestComputeLevelSize (SquareLogoSize, 0));
        }

        [Test]
        public void ComputeLevelSize_PowerOfTwo ()
        {
            Assert.AreEqual (new Size (1024, 512), TestComputeLevelSize (PowerOfTwoSize, 11));
            Assert.AreEqual (new Size (1024, 512), TestComputeLevelSize (PowerOfTwoSize, 10));
            Assert.AreEqual (new Size (512, 256), TestComputeLevelSize (PowerOfTwoSize, 9));
            Assert.AreEqual (new Size (256, 128), TestComputeLevelSize (PowerOfTwoSize, 8));
            Assert.AreEqual (new Size (128, 64), TestComputeLevelSize (PowerOfTwoSize, 7));
            Assert.AreEqual (new Size (64, 32), TestComputeLevelSize (PowerOfTwoSize, 6));
            Assert.AreEqual (new Size (32, 16), TestComputeLevelSize (PowerOfTwoSize, 5));
            Assert.AreEqual (new Size (16, 8), TestComputeLevelSize (PowerOfTwoSize, 4));
            Assert.AreEqual (new Size (8, 4), TestComputeLevelSize (PowerOfTwoSize, 3));
            Assert.AreEqual (new Size (4, 2), TestComputeLevelSize (PowerOfTwoSize, 2));
            Assert.AreEqual (new Size (2, 1), TestComputeLevelSize (PowerOfTwoSize, 1));
            Assert.AreEqual (new Size (1, 1), TestComputeLevelSize (PowerOfTwoSize, 0));
        }

        private static IEnumerable<Tile> TestComputeTiles (Size levelSize, int tileSize, int tileOverlap)
        {
            var settings = new Settings(new SettingsBuilder
            {
                TileSize = tileSize,
                TileOverlap = tileOverlap,
            });
            var dzi = new DeepZoomImage (settings);
            return dzi.ComputeTiles (levelSize);
        }

        [Test]
        public void ComputeTiles_SmallerThanTile ()
        {
            var size = new Size(150, 188);
            var actual = TestComputeTiles (size, 254, 1);
            var expected = new[] {new Tile (size, 0, 0)};
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_SizeOfTile ()
        {
            var size = new Size (254, 254);
            var actual = TestComputeTiles (size, 254, 1);
            var expected = new[] {new Tile (size, 0, 0)};
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BiggerThanTile ()
        {
            var size = new Size (300, 375);
            var actual = TestComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Tile (  0,   0, 254, 254, 0, 0),
                new Tile (  0, 253, 254, 374, 1, 0),

                new Tile (253,   0, 299, 254, 0, 1),
                new Tile (253, 253, 299, 374, 1, 1),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BigWithDoubleOverlap ()
        {
            var size = new Size (700, 700);
            var actual = TestComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Tile (  0,   0, 254, 254, 0, 0),
                new Tile (  0, 253, 254, 508, 1, 0),
                new Tile (  0, 507, 254, 699, 2, 0),

                new Tile (253,   0, 508, 254, 0, 1),
                new Tile (253, 253, 508, 508, 1, 1),
                new Tile (253, 507, 508, 699, 2, 1),

                new Tile (507,   0, 699, 254, 0, 2),
                new Tile (507, 253, 699, 508, 1, 2),
                new Tile (507, 507, 699, 699, 2, 2),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BiggerThanTileWithOverlapOfTwo ()
        {
            var size = new Size (300, 375);
            var actual = TestComputeTiles (size, 254, 2);
            var expected = new[]
            {
                new Tile (  0,   0, 255, 255, 0, 0),
                new Tile (  0, 252, 255, 374, 1, 0),

                new Tile (252,   0, 299, 255, 0, 1),
                new Tile (252, 252, 299, 374, 1, 1),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_BiggerThanTileWithOverlapOfThree ()
        {
            var size = new Size (300, 375);
            var actual = TestComputeTiles (size, 254, 3);
            var expected = new[]
            {
                new Tile (  0,   0, 256, 256, 0, 0),
                new Tile (  0, 251, 256, 374, 1, 0),

                new Tile (251,   0, 299, 256, 0, 1),
                new Tile (251, 251, 299, 374, 1, 1),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_OnePixelBiggerThanTile ()
        {
            var size = new Size (255, 255);
            var actual = TestComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Tile (  0,   0, 254, 254, 0, 0),
                new Tile (  0, 253, 254, 254, 1, 0),

                new Tile (253,   0, 254, 254, 0, 1),
                new Tile (253, 253, 254, 254, 1, 1),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_OnePixelBiggerThanTileWithOverlapOfTwo ()
        {
            var size = new Size (255, 255);
            var actual = TestComputeTiles (size, 254, 2);
            var expected = new[]
            {
                new Tile (  0,   0, 254, 254, 0, 0),
                new Tile (  0, 252, 254, 254, 1, 0),

                new Tile (252,   0, 254, 254, 0, 1),
                new Tile (252, 252, 254, 254, 1, 1),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_TwoPixelsBiggerThanTile ()
        {
            var size = new Size (256, 256);
            var actual = TestComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Tile (  0,   0, 254, 254, 0, 0),
                new Tile (  0, 253, 254, 255, 1, 0),

                new Tile (253,   0, 255, 254, 0, 1),
                new Tile (253, 253, 255, 255, 1, 1),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_TwoPixelsBiggerThanTileWithOverlapOfTwo ()
        {
            var size = new Size (256, 256);
            var actual = TestComputeTiles (size, 254, 2);
            var expected = new[]
            {
                new Tile (  0,   0, 255, 255, 0, 0),
                new Tile (  0, 252, 255, 255, 1, 0),

                new Tile (252,   0, 255, 255, 0, 1),
                new Tile (252, 252, 255, 255, 1, 1),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_OriginalSize ()
        {
            var size = new Size (1200, 1500);
            var actual = TestComputeTiles (size, 254, 1);
            var expected = new[]
            {
                new Tile (   0,    0,  254,  254, 0, 0),
                new Tile (   0,  253,  254,  508, 1, 0),
                new Tile (   0,  507,  254,  762, 2, 0),
                new Tile (   0,  761,  254, 1016, 3, 0),
                new Tile (   0, 1015,  254, 1270, 4, 0),
                new Tile (   0, 1269,  254, 1499, 5, 0),

                new Tile ( 253,    0,  508,  254, 0, 1),
                new Tile ( 253,  253,  508,  508, 1, 1),
                new Tile ( 253,  507,  508,  762, 2, 1),
                new Tile ( 253,  761,  508, 1016, 3, 1),
                new Tile ( 253, 1015,  508, 1270, 4, 1),
                new Tile ( 253, 1269,  508, 1499, 5, 1),

                new Tile ( 507,    0,  762,  254, 0, 2),
                new Tile ( 507,  253,  762,  508, 1, 2),
                new Tile ( 507,  507,  762,  762, 2, 2),
                new Tile ( 507,  761,  762, 1016, 3, 2),
                new Tile ( 507, 1015,  762, 1270, 4, 2),
                new Tile ( 507, 1269,  762, 1499, 5, 2),

                new Tile ( 761,    0, 1016,  254, 0, 3),
                new Tile ( 761,  253, 1016,  508, 1, 3),
                new Tile ( 761,  507, 1016,  762, 2, 3),
                new Tile ( 761,  761, 1016, 1016, 3, 3),
                new Tile ( 761, 1015, 1016, 1270, 4, 3),
                new Tile ( 761, 1269, 1016, 1499, 5, 3),

                new Tile (1015,    0, 1199,  254, 0, 4),
                new Tile (1015,  253, 1199,  508, 1, 4),
                new Tile (1015,  507, 1199,  762, 2, 4),
                new Tile (1015,  761, 1199, 1016, 3, 4),
                new Tile (1015, 1015, 1199, 1270, 4, 4),
                new Tile (1015, 1269, 1199, 1499, 5, 4),
            };
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ComputeTiles_AllLevelsOfSquareLogo ()
        {
            var tester = new Action<int, int, int> ((inputSide, expectedTileCount, expectedFirstTileSize) =>
                {
                    var tiles = TestComputeTiles (new Size (inputSide, inputSide), 254, 1);
                    var e = tiles.GetEnumerator ();
                    e.MoveNext ();
                    var firstTile = e.Current;
                    var actualRectangle = firstTile.Rectangle;
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
                new Tile (  0,   0, 254, 254, 0, 0),
                new Tile (  0, 253, 254, 374, 1, 0),

                new Tile (253,   0, 299, 254, 0, 1),
                new Tile (253, 253, 299, 374, 1, 1),
            };
            var streams = new Dictionary<string, MemoryStream>
            {
                {"0_0", new MemoryStream()},
                {"0_1", new MemoryStream()},
                {"1_0", new MemoryStream()},
                {"1_1", new MemoryStream()},
            };
            var settings = new Settings(new SettingsBuilder { PostImageEncoding = ImageFormat.Png, });
            var dzi = new DeepZoomImage (settings);

            try
            {
                using (var inputStream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomImageTest> ("300x375.png"))
                using (var sourceBitmap = new Bitmap (inputStream))
                {
                    dzi.Slice (sourceBitmap, tiles, tilename => streams[tilename]);
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
                        var levelSize = TestComputeLevelSize (SquareLogoSize, level);
                        var tiles = TestComputeTiles (levelSize, 254, 1);
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
