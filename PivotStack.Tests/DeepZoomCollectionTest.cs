using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using SoftwareNinjas.Core;
using Test = SoftwareNinjas.Core.Test;
using NUnit.Framework;

namespace PivotStack.Tests
{
    [TestFixture]
    public class DeepZoomCollectionTest
    {
        private static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            NewLineChars = "\n",
#if DEBUG
            Indent = true,
            IndentChars = "  ",
#else
            NewLineHandling = NewLineHandling.Entitize,
#endif
        };

        private static readonly int[] TestIds = new[]
        {
            000, 010, 020, 030, 040, 050, 060, 070, 080, 090,
            100, 110, 120, 130, 140, 150, 160, 170, 180, 190,
            200, 210, 220, 230, 240, 250, 260, 270, 280, 290
        };

        [Test]
        public void GenerateCollectionTiles_OneToThirtyTwo ()
        {
            var expected = new[]
            {
                new ImageCollectionTile (0, 0, 0, new[]
                    {
                        TestIds[ 0],
                        TestIds[ 1],
                        TestIds[ 2],
                        TestIds[ 3],
                        TestIds[ 4],
                        TestIds[ 5],
                        TestIds[ 6],
                        TestIds[ 7],
                        TestIds[ 8],
                        TestIds[ 9],
                        TestIds[10],
                        TestIds[11],
                        TestIds[12],
                        TestIds[13],
                        TestIds[14],
                        TestIds[15],
                        TestIds[16],
                        TestIds[17],
                        TestIds[18],
                        TestIds[19],
                        TestIds[20],
                        TestIds[21],
                        TestIds[22],
                        TestIds[23],
                        TestIds[24],
                        TestIds[25],
                        TestIds[26],
                        TestIds[27],
                        TestIds[28],
                        TestIds[29],
                    }
                ),
            };
            for (var i = 0; i < 6; i++)
            {
                var levelSize = (int) Math.Pow(2, i);
                var actual = DeepZoomCollection.GenerateCollectionTiles (TestIds, levelSize);
                Test.EnumerableExtensions.EnumerateSame (expected, actual);
            }
        }

        [Test]
        public void GenerateCollectionTiles_SixtyFour ()
        {
            var expected = new[]
            {
                new ImageCollectionTile (0, 0, 0, new[]
                    {
                        TestIds[ 0],
                        TestIds[ 1],
                        TestIds[ 2],
                        TestIds[ 3],
                        TestIds[ 4],
                        TestIds[ 5],
                        TestIds[ 6],
                        TestIds[ 7],
                        TestIds[ 8],
                        TestIds[ 9],
                        TestIds[10],
                        TestIds[11],
                        TestIds[12],
                        TestIds[13],
                        TestIds[14],
                        TestIds[15],
                    }
                ),
                new ImageCollectionTile (0, 1, 16, new[]
                    {
                        TestIds[16],
                        TestIds[17],
                        TestIds[18],
                        TestIds[19],
                        TestIds[20],
                        TestIds[21],
                        TestIds[22],
                        TestIds[23],
                        TestIds[24],
                        TestIds[25],
                        TestIds[26],
                        TestIds[27],
                        TestIds[28],
                        TestIds[29],
                    }
                ),
            };
            var actual = DeepZoomCollection.GenerateCollectionTiles (TestIds, 64);
            Test.EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void GenerateCollectionTiles_OneTwentyEight ()
        {
            var expected = new[]
            {
                new ImageCollectionTile (0, 0, 0, new[]
                    {
                        TestIds[ 0],
                        TestIds[ 1],
                        TestIds[ 2],
                        TestIds[ 3],
                    }
                ),
                new ImageCollectionTile (0, 1, 4, new[]
                    {
                        TestIds[ 4],
                        TestIds[ 5],
                        TestIds[ 6],
                        TestIds[ 7],
                    }
                ),
                new ImageCollectionTile (1, 0, 8, new[]
                    {
                        TestIds[ 8],
                        TestIds[ 9],
                        TestIds[10],
                        TestIds[11],
                    }
                ),
                new ImageCollectionTile (1, 1, 12, new[]
                    {
                        TestIds[12],
                        TestIds[13],
                        TestIds[14],
                        TestIds[15],
                    }
                ),
                new ImageCollectionTile (0, 2, 16, new[]
                    {
                        TestIds[16],
                        TestIds[17],
                        TestIds[18],
                        TestIds[19],
                    }
                ),
                new ImageCollectionTile (0, 3, 20, new[]
                    {
                        TestIds[20],
                        TestIds[21],
                        TestIds[22],
                        TestIds[23],
                    }
                ),
                new ImageCollectionTile (1, 2, 24, new[]
                    {
                        TestIds[24],
                        TestIds[25],
                        TestIds[26],
                        TestIds[27],
                    }
                ),
                new ImageCollectionTile (1, 3, 28, new[]
                    {
                        TestIds[28],
                        TestIds[29],
                    }
                ),
            };
            var actual = DeepZoomCollection.GenerateCollectionTiles (TestIds, 128);
            Test.EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void GenerateCollectionTiles_TwoFiftySix ()
        {
            var expected = new[]
            {
                new ImageCollectionTile (0, 0, 0, new[]
                    {
                        TestIds[ 0],
                    }
                ),
                new ImageCollectionTile (0, 1, 1, new[]
                    {
                        TestIds[ 1],
                    }
                ),
                new ImageCollectionTile (1, 0, 2, new[]
                    {
                        TestIds[ 2],
                    }
                ),
                new ImageCollectionTile (1, 1, 3, new[]
                    {
                        TestIds[ 3],
                    }
                ),
                new ImageCollectionTile (0, 2, 4, new[]
                    {
                        TestIds[ 4],
                    }
                ),
                new ImageCollectionTile (0, 3, 5, new[]
                    {
                        TestIds[ 5],
                    }
                ),
                new ImageCollectionTile (1, 2, 6, new[]
                    {
                        TestIds[ 6],
                    }
                ),
                new ImageCollectionTile (1, 3, 7, new[]
                    {
                        TestIds[ 7],
                    }
                ),
                new ImageCollectionTile (2, 0, 8, new[]
                    {
                        TestIds[ 8],
                    }
                ),
                new ImageCollectionTile (2, 1, 9, new[]
                    {
                        TestIds[ 9],
                    }
                ),
                new ImageCollectionTile (3, 0, 10, new[]
                    {
                        TestIds[10],
                    }
                ),
                new ImageCollectionTile (3, 1, 11, new[]
                    {
                        TestIds[11],
                    }
                ),
                new ImageCollectionTile (2, 2, 12, new[]
                    {
                        TestIds[12],
                    }
                ),
                new ImageCollectionTile (2, 3, 13, new[]
                    {
                        TestIds[13],
                    }
                ),
                new ImageCollectionTile (3, 2, 14, new[]
                    {
                        TestIds[14],
                    }
                ),
                new ImageCollectionTile (3, 3, 15, new[]
                    {
                        TestIds[15],
                    }
                ),
                new ImageCollectionTile (0, 4, 16, new[]
                    {
                        TestIds[16],
                    }
                ),
                new ImageCollectionTile (0, 5, 17, new[]
                    {
                        TestIds[17],
                    }
                ),
                new ImageCollectionTile (1, 4, 18, new[]
                    {
                        TestIds[18],
                    }
                ),
                new ImageCollectionTile (1, 5, 19, new[]
                    {
                        TestIds[19],
                    }
                ),
                new ImageCollectionTile (0, 6, 20, new[]
                    {
                        TestIds[20],
                    }
                ),
                new ImageCollectionTile (0, 7, 21, new[]
                    {
                        TestIds[21],
                    }
                ),
                new ImageCollectionTile (1, 6, 22, new[]
                    {
                        TestIds[22],
                    }
                ),
                new ImageCollectionTile (1, 7, 23, new[]
                    {
                        TestIds[23],
                    }
                ),
                new ImageCollectionTile (2, 4, 24, new[]
                    {
                        TestIds[24],
                    }
                ),
                new ImageCollectionTile (2, 5, 25, new[]
                    {
                        TestIds[25],
                    }
                ),
                new ImageCollectionTile (3, 4, 26, new[]
                    {
                        TestIds[26],
                    }
                ),
                new ImageCollectionTile (3, 5, 27, new[]
                    {
                        TestIds[27],
                    }
                ),
                new ImageCollectionTile (2, 6, 28, new[]
                    {
                        TestIds[28],
                    }
                ),
                new ImageCollectionTile (2, 7, 29, new[]
                    {
                        TestIds[29],
                    }
                ),
            };
            var actual = DeepZoomCollection.GenerateCollectionTiles (TestIds, 256);
            Test.EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void CreateCollectionTile()
        {
            using (var oneStream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomCollectionTest> ("1.png"))
            using (var one = new Bitmap(oneStream))
            using (var twoStream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomCollectionTest> ("2.png"))
            using (var two = new Bitmap (twoStream))
            using (var threeStream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomCollectionTest> ("3.png"))
            using (var three = new Bitmap (threeStream))
            using (var fourStream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomCollectionTest> ("4.png"))
            using (var four = new Bitmap (fourStream))
            {
                var sourceBitmaps = new[] {one, two, three, four};
                using (var actualBitmap = DeepZoomCollection.CreateCollectionTile (sourceBitmaps, 128))
                using (var actualStream = new MemoryStream())
                {
                    actualBitmap.Save (actualStream, ImageFormat.Png);

                    ProgramTest.AssertStreamsAreEqual<DeepZoomCollectionTest> ("1234.png", actualStream);
                }
            }
        }

        [Test]
        public void GenerateImageCollection ()
        {
            var settings = new Settings
            {
                HighestId = 4000,
                PostImageEncoding = ImageFormat.Png,
                ItemImageSize = new Size(800, 400),
                XmlWriterSettings = XmlWriterSettings,
            };
            var dzc = new DeepZoomCollection (settings);

            const string expectedXml = @"
<Collection xmlns='http://schemas.microsoft.com/deepzoom/2008' MaxLevel='7' TileSize='256' Format='png' NextItemId='111'>
  <Items>
    <I N='0' Id='1' Source='../0/0001.dzi'>
      <Size Width='800' Height='400' />
    </I>
    <I N='1' Id='51' Source='../0/0051.dzi'>
      <Size Width='800' Height='400' />
    </I>
    <I N='2' Id='110' Source='../0/0110.dzi'>
      <Size Width='800' Height='400' />
    </I>
  </Items>
</Collection>";
            var expectedCollectionElement = XElement.Parse (expectedXml);

            var postIds = new[] { 1, 51, 110 };
            var actualCollectionElement = dzc.GenerateImageCollection (postIds, "../");

            Assert.AreEqual (expectedCollectionElement.ToString (), actualCollectionElement.ToString ());
        }

        [Test]
        public void CreateImageCollectionItemNode_Typical ()
        {
            var settings = new Settings
            {
                HighestId = 4000,
            };
            var dzc = new DeepZoomCollection (settings);
            const string expectedXml = @"
<I N='0' Id='351' Source='../../..\0/0351.dzi' xmlns='http://schemas.microsoft.com/deepzoom/2008' />";
            var expectedItemNode = XElement.Parse (expectedXml);

            var actualItemNode = dzc.CreateImageCollectionItemNode (0, 351, "../../..");

            Assert.AreEqual (expectedItemNode.ToString (), actualItemNode.ToString ());
        }
    }
}
