using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

using SoftwareNinjas.Core;

using Tile = SoftwareNinjas.Core.Pair<System.Windows.Rect, string>;

namespace PivotStack
{
    public class DeepZoomImage
    {
        internal const string TileNameTemplate = "{0}_{1}";
        internal static readonly string TileZeroZero = "0_0";
        internal static readonly Size OneByOne = new Size (1, 1);

        internal static int DetermineMaximumLevel (Size originalSize)
        {
            var maxDimension = Math.Max (originalSize.Height, originalSize.Width);
            return (int) Math.Ceiling (Math.Log (maxDimension, 2));
        }

        // TODO: Consider accepting a maximumLevel parameter, obtained by calling DetermineMaximumLevel()
        internal static Size ComputeLevelSize(Size originalSize, int levelNumber)
        {
            if (levelNumber < 0)
            {
                throw new ArgumentOutOfRangeException("levelNumber", levelNumber, "levelNumber must be >= 0");
            }

            Size result;
            if (0 == levelNumber)
            {
                result = OneByOne; 
            }
            else
            {
                int maxLevel = DetermineMaximumLevel (originalSize);
                if (levelNumber >= maxLevel)
                {
                    result = originalSize;
                }
                else
                {
                    // shifting does not account for rounding, so we divide and round up (ceiling)
                    var levelDifference = maxLevel - levelNumber;
                    var divisor = Math.Pow (2, levelDifference);
                    var width = (int) Math.Ceiling (originalSize.Width / divisor);
                    var height = (int) Math.Ceiling (originalSize.Height / divisor);
                    result = new Size(width, height);
                }
            }

            return result;
        }

        internal static string TileName(int row, int column)
        {
            if (0 == row && 0 == column)
            {
                return TileZeroZero;
            }
            return TileNameTemplate.FormatInvariant (column, row);
        }

        internal static IEnumerable<Tile> ComputeTiles(Size levelSize, int tileSize, int tileOverlap)
        {
            var width = levelSize.Width;
            var height = levelSize.Height;
            var maxDimension = Math.Max (width, height);
            if (maxDimension <= tileSize)
            {
                var pair = new Tile (new Rect(levelSize), TileZeroZero);
                yield return pair;
            }
            else
            {
                var columns = Math.Ceiling (width / tileSize);
                var rows = Math.Ceiling (height / tileSize);
                var tileOffsetMultiplier = tileSize + tileOverlap - 1;

                for (int column = 0; column < columns; column++)
                {
                    var left = 0 == column ? 0 : column * tileSize - tileOverlap;
                    var right = Math.Min (width - 1, (column + 1) * tileOffsetMultiplier);

                    for (int row = 0; row < rows; row++)
                    {
                        var top = 0 == row ? 0 : row * tileSize - tileOverlap;
                        var bottom = Math.Min (height - 1, (row + 1) * tileOffsetMultiplier);

                        var rect = new Rect (new Point (left, top), new Point (right, bottom));
                        var tileName = TileName (row, column);
                        yield return new Tile (rect, tileName);
                    }
                }
            }
        }

        // TODO: Looks like DrawImage() could be used to resize and slice into tiles at the same time...?
        internal static Bitmap Resize(Bitmap sourceImage, int targetWidth, int targetHeight)
        {
            var targetImage = new Bitmap (targetWidth, targetHeight);

            using (var graphics = Graphics.FromImage (targetImage) )
            {
                graphics.InterpolationMode = InterpolationMode.Default;
                graphics.DrawImage (
                    sourceImage,
                    new Rectangle (0, 0, targetWidth, targetHeight),
                    0,
                    0,
                    sourceImage.Width,
                    sourceImage.Height,
                    GraphicsUnit.Pixel
                );
            }

            return targetImage;
        }
    }
}
