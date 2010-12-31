using System.Collections.Generic;

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
            var imagesInEachDimension = NumberOfImagesInEachDimension (levelSize);
            var imagesPerTile = imagesInEachDimension * imagesInEachDimension;
            var mortonNumber = 0;
            var imagesThisTile = 0;

            var currentRow = 0;
            var currentColumn = 0;
            var idsForTile = new List<int> ();
            var startingMortonNumber = 0;
            foreach (var id in ids)
            {
                idsForTile.Add (id);

                mortonNumber++;
                imagesThisTile++;
                if (imagesThisTile == imagesPerTile)
                {
                    var imageCollectionTile = 
                        new ImageCollectionTile (currentRow, currentColumn, startingMortonNumber, idsForTile);
                    yield return imageCollectionTile;
                    startingMortonNumber = mortonNumber;
                    imagesThisTile = 0;
                    var point = MortonLayout.Decode (mortonNumber);
                    currentColumn = point.X / imagesInEachDimension;
                    currentRow = point.Y / imagesInEachDimension;
                    idsForTile.Clear ();
                }
            }
            if (imagesThisTile > 0)
            {
                var imageCollectionTile = 
                    new ImageCollectionTile (currentRow, currentColumn, startingMortonNumber, idsForTile);
                yield return imageCollectionTile;
            }
        }

        internal static int NumberOfImagesInEachDimension (int levelSize)
        {
            return CollectionTileSize / levelSize;
        }
    }
}
