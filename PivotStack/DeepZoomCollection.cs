using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PivotStack
{
    public class DeepZoomCollection
    {
        internal const int CollectionTilePower = 8;
        internal const int CollectionTileSize = 256;

        internal static Bitmap CreateCollectionTile(IEnumerable<Bitmap> componentBitmaps, int levelSize)
        {
            var result = new Bitmap (CollectionTileSize, CollectionTileSize);
            using (var graphics = Graphics.FromImage (result))
            {
                graphics.InterpolationMode = InterpolationMode.Default;
                var mortonNumber = 0;
                foreach (var itemBitmap in componentBitmaps)
                {
                    var mortonLocation = MortonLayout.Decode (mortonNumber);
                    var destRect = new Rectangle (
                        mortonLocation.X * levelSize,
                        mortonLocation.Y * levelSize,
                        levelSize,
                        levelSize
                    );
                    graphics.DrawImage (
                        itemBitmap,
                        destRect,
                        0,
                        0,
                        levelSize,
                        levelSize,
                        GraphicsUnit.Pixel
                    );
                    mortonNumber++;
                }
            }
            return result;
        }

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
