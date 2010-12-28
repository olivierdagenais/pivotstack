using System;
using System.Collections.Generic;
using SoftwareNinjas.Core;
using Test = SoftwareNinjas.Core.Test;
using NUnit.Framework;

namespace PivotStack.Tests
{
    [TestFixture]
    public class DeepZoomCollectionTest
    {
        private static readonly int[] TestIds = new[]
        {
            000, 010, 020, 030, 040, 050, 060, 070, 080, 090,
            100, 110, 120, 130, 140, 150, 160, 170, 180, 190,
            200, 210, 220, 230, 240, 250, 260, 270, 280, 290
        };

        private static readonly IList<Pair<int, int>> IdsToMortonNumbers = new List<Pair<int, int>>
        {
                new Pair<int, int>(000,  0),
                new Pair<int, int>(010,  1),
                new Pair<int, int>(020,  2),
                new Pair<int, int>(030,  3),
                new Pair<int, int>(040,  4),
                new Pair<int, int>(050,  5),
                new Pair<int, int>(060,  6),
                new Pair<int, int>(070,  7),
                new Pair<int, int>(080,  8),
                new Pair<int, int>(090,  9),
                new Pair<int, int>(100, 10),
                new Pair<int, int>(110, 11),
                new Pair<int, int>(120, 12),
                new Pair<int, int>(130, 13),
                new Pair<int, int>(140, 14),
                new Pair<int, int>(150, 15),
                new Pair<int, int>(160, 16),
                new Pair<int, int>(170, 17),
                new Pair<int, int>(180, 18),
                new Pair<int, int>(190, 19),
                new Pair<int, int>(200, 20),
                new Pair<int, int>(210, 21),
                new Pair<int, int>(220, 22),
                new Pair<int, int>(230, 23),
                new Pair<int, int>(240, 24),
                new Pair<int, int>(250, 25),
                new Pair<int, int>(260, 26),
                new Pair<int, int>(270, 27),
                new Pair<int, int>(280, 28),
                new Pair<int, int>(290, 29),
        };

        [Test]
        public void GenerateCollectionTiles_OneToThirtyTwo ()
        {
            var expected = new[]
            {
                new ImageCollectionTile (0, 0, new[]
                    {
                        IdsToMortonNumbers[ 0],
                        IdsToMortonNumbers[ 1],
                        IdsToMortonNumbers[ 2],
                        IdsToMortonNumbers[ 3],
                        IdsToMortonNumbers[ 4],
                        IdsToMortonNumbers[ 5],
                        IdsToMortonNumbers[ 6],
                        IdsToMortonNumbers[ 7],
                        IdsToMortonNumbers[ 8],
                        IdsToMortonNumbers[ 9],
                        IdsToMortonNumbers[10],
                        IdsToMortonNumbers[11],
                        IdsToMortonNumbers[12],
                        IdsToMortonNumbers[13],
                        IdsToMortonNumbers[14],
                        IdsToMortonNumbers[15],
                        IdsToMortonNumbers[16],
                        IdsToMortonNumbers[17],
                        IdsToMortonNumbers[18],
                        IdsToMortonNumbers[19],
                        IdsToMortonNumbers[20],
                        IdsToMortonNumbers[21],
                        IdsToMortonNumbers[22],
                        IdsToMortonNumbers[23],
                        IdsToMortonNumbers[24],
                        IdsToMortonNumbers[25],
                        IdsToMortonNumbers[26],
                        IdsToMortonNumbers[27],
                        IdsToMortonNumbers[28],
                        IdsToMortonNumbers[29],
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
                new ImageCollectionTile (0, 0, new[]
                    {
                        IdsToMortonNumbers[ 0],
                        IdsToMortonNumbers[ 1],
                        IdsToMortonNumbers[ 2],
                        IdsToMortonNumbers[ 3],
                        IdsToMortonNumbers[ 4],
                        IdsToMortonNumbers[ 5],
                        IdsToMortonNumbers[ 6],
                        IdsToMortonNumbers[ 7],
                        IdsToMortonNumbers[ 8],
                        IdsToMortonNumbers[ 9],
                        IdsToMortonNumbers[10],
                        IdsToMortonNumbers[11],
                        IdsToMortonNumbers[12],
                        IdsToMortonNumbers[13],
                        IdsToMortonNumbers[14],
                        IdsToMortonNumbers[15],
                    }
                ),
                new ImageCollectionTile (0, 1, new[]
                    {
                        IdsToMortonNumbers[16],
                        IdsToMortonNumbers[17],
                        IdsToMortonNumbers[18],
                        IdsToMortonNumbers[19],
                        IdsToMortonNumbers[20],
                        IdsToMortonNumbers[21],
                        IdsToMortonNumbers[22],
                        IdsToMortonNumbers[23],
                        IdsToMortonNumbers[24],
                        IdsToMortonNumbers[25],
                        IdsToMortonNumbers[26],
                        IdsToMortonNumbers[27],
                        IdsToMortonNumbers[28],
                        IdsToMortonNumbers[29],
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
                new ImageCollectionTile (0, 0, new[]
                    {
                        IdsToMortonNumbers[ 0],
                        IdsToMortonNumbers[ 1],
                        IdsToMortonNumbers[ 2],
                        IdsToMortonNumbers[ 3],
                    }
                ),
                new ImageCollectionTile (0, 1, new[]
                    {
                        IdsToMortonNumbers[ 4],
                        IdsToMortonNumbers[ 5],
                        IdsToMortonNumbers[ 6],
                        IdsToMortonNumbers[ 7],
                    }
                ),
                new ImageCollectionTile (1, 0, new[]
                    {
                        IdsToMortonNumbers[ 8],
                        IdsToMortonNumbers[ 9],
                        IdsToMortonNumbers[10],
                        IdsToMortonNumbers[11],
                    }
                ),
                new ImageCollectionTile (1, 1, new[]
                    {
                        IdsToMortonNumbers[12],
                        IdsToMortonNumbers[13],
                        IdsToMortonNumbers[14],
                        IdsToMortonNumbers[15],
                    }
                ),
                new ImageCollectionTile (0, 2, new[]
                    {
                        IdsToMortonNumbers[16],
                        IdsToMortonNumbers[17],
                        IdsToMortonNumbers[18],
                        IdsToMortonNumbers[19],
                    }
                ),
                new ImageCollectionTile (0, 3, new[]
                    {
                        IdsToMortonNumbers[20],
                        IdsToMortonNumbers[21],
                        IdsToMortonNumbers[22],
                        IdsToMortonNumbers[23],
                    }
                ),
                new ImageCollectionTile (1, 2, new[]
                    {
                        IdsToMortonNumbers[24],
                        IdsToMortonNumbers[25],
                        IdsToMortonNumbers[26],
                        IdsToMortonNumbers[27],
                    }
                ),
                new ImageCollectionTile (1, 3, new[]
                    {
                        IdsToMortonNumbers[28],
                        IdsToMortonNumbers[29],
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
                new ImageCollectionTile (0, 0, new[]
                    {
                        IdsToMortonNumbers[ 0],
                    }
                ),
                new ImageCollectionTile (0, 1, new[]
                    {
                        IdsToMortonNumbers[ 1],
                    }
                ),
                new ImageCollectionTile (1, 0, new[]
                    {
                        IdsToMortonNumbers[ 2],
                    }
                ),
                new ImageCollectionTile (1, 1, new[]
                    {
                        IdsToMortonNumbers[ 3],
                    }
                ),
                new ImageCollectionTile (0, 2, new[]
                    {
                        IdsToMortonNumbers[ 4],
                    }
                ),
                new ImageCollectionTile (0, 3, new[]
                    {
                        IdsToMortonNumbers[ 5],
                    }
                ),
                new ImageCollectionTile (1, 2, new[]
                    {
                        IdsToMortonNumbers[ 6],
                    }
                ),
                new ImageCollectionTile (1, 3, new[]
                    {
                        IdsToMortonNumbers[ 7],
                    }
                ),
                new ImageCollectionTile (2, 0, new[]
                    {
                        IdsToMortonNumbers[ 8],
                    }
                ),
                new ImageCollectionTile (2, 1, new[]
                    {
                        IdsToMortonNumbers[ 9],
                    }
                ),
                new ImageCollectionTile (3, 0, new[]
                    {
                        IdsToMortonNumbers[10],
                    }
                ),
                new ImageCollectionTile (3, 1, new[]
                    {
                        IdsToMortonNumbers[11],
                    }
                ),
                new ImageCollectionTile (2, 2, new[]
                    {
                        IdsToMortonNumbers[12],
                    }
                ),
                new ImageCollectionTile (2, 3, new[]
                    {
                        IdsToMortonNumbers[13],
                    }
                ),
                new ImageCollectionTile (3, 2, new[]
                    {
                        IdsToMortonNumbers[14],
                    }
                ),
                new ImageCollectionTile (3, 3, new[]
                    {
                        IdsToMortonNumbers[15],
                    }
                ),
                new ImageCollectionTile (0, 4, new[]
                    {
                        IdsToMortonNumbers[16],
                    }
                ),
                new ImageCollectionTile (0, 5, new[]
                    {
                        IdsToMortonNumbers[17],
                    }
                ),
                new ImageCollectionTile (1, 4, new[]
                    {
                        IdsToMortonNumbers[18],
                    }
                ),
                new ImageCollectionTile (1, 5, new[]
                    {
                        IdsToMortonNumbers[19],
                    }
                ),
                new ImageCollectionTile (0, 6, new[]
                    {
                        IdsToMortonNumbers[20],
                    }
                ),
                new ImageCollectionTile (0, 7, new[]
                    {
                        IdsToMortonNumbers[21],
                    }
                ),
                new ImageCollectionTile (1, 6, new[]
                    {
                        IdsToMortonNumbers[22],
                    }
                ),
                new ImageCollectionTile (1, 7, new[]
                    {
                        IdsToMortonNumbers[23],
                    }
                ),
                new ImageCollectionTile (2, 4, new[]
                    {
                        IdsToMortonNumbers[24],
                    }
                ),
                new ImageCollectionTile (2, 5, new[]
                    {
                        IdsToMortonNumbers[25],
                    }
                ),
                new ImageCollectionTile (3, 4, new[]
                    {
                        IdsToMortonNumbers[26],
                    }
                ),
                new ImageCollectionTile (3, 5, new[]
                    {
                        IdsToMortonNumbers[27],
                    }
                ),
                new ImageCollectionTile (2, 6, new[]
                    {
                        IdsToMortonNumbers[28],
                    }
                ),
                new ImageCollectionTile (2, 7, new[]
                    {
                        IdsToMortonNumbers[29],
                    }
                ),
            };
            var actual = DeepZoomCollection.GenerateCollectionTiles (TestIds, 256);
            Test.EnumerableExtensions.EnumerateSame (expected, actual);
        }
    }
}
