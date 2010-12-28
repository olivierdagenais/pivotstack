using System.Collections.Generic;
using SoftwareNinjas.Core;

namespace PivotStack
{
    public class DeepZoomCollection
    {
        internal const int CollectionTileSize = 256;

        internal static IEnumerable<ImageCollectionTile> GenerateCollectionTiles (
            IEnumerable<int> ids,
            int levelSize
        )
        {
            var imagesInEachDimension = CollectionTileSize / levelSize;
            var imagesPerTile = imagesInEachDimension * imagesInEachDimension;
            var mortonNumber = 0;
            var imagesThisTile = 0;

            var currentRow = 0;
            var currentColumn = 0;
            var idsAndMortonNumbers = new List<Pair<int, int>> ();
            foreach (var id in ids)
            {
                idsAndMortonNumbers.Add (new Pair<int, int> (id, mortonNumber));

                mortonNumber++;
                imagesThisTile++;
                if (imagesThisTile == imagesPerTile)
                {
                    var imageCollectionTile = new ImageCollectionTile (currentRow, currentColumn, idsAndMortonNumbers);
                    yield return imageCollectionTile;
                    imagesThisTile = 0;
                    var point = MortonLayout.Decode (mortonNumber);
                    currentColumn = point.X / imagesInEachDimension;
                    currentRow = point.Y / imagesInEachDimension;
                    idsAndMortonNumbers = new List<Pair<int, int>> ();
                }
            }
            if (imagesThisTile > 0)
            {
                var imageCollectionTile = new ImageCollectionTile (currentRow, currentColumn, idsAndMortonNumbers);
                yield return imageCollectionTile;
            }
        }
    }
}
