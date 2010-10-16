using System;
using System.Collections.Generic;
using System.Windows;
using SoftwareNinjas.Core;

namespace PivotStack
{
    public class DeepZoomImage
    {
        internal const string TileNameTemplate = "{0}_{1}";
        internal static readonly string TileZeroZero = "0_0";
        internal static readonly Size OneByOne = new Size (1, 1);

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
                var maxDimension = Math.Max (originalSize.Height, originalSize.Width);
                var maxLevel = (int) Math.Ceiling (Math.Log (maxDimension, 2));
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

        internal static IEnumerable<Pair<Rect, string>> ComputeTiles(Size levelSize, int tileSize, int tileOverlap)
        {
            var width = levelSize.Width;
            var height = levelSize.Height;
            var maxDimension = Math.Max (width, height);
            if (maxDimension <= tileSize)
            {
                var pair = new Pair<Rect, string> (new Rect(levelSize), TileZeroZero);
                yield return pair;
            }
            else
            {
                var columns = Math.Ceiling (width / tileSize);
                var rows = Math.Ceiling (height / tileSize);

                for (int column = 0; column < columns; column++)
                {
                    var left = 0 == column ? 0 : column * tileSize - tileOverlap;
                    var right = Math.Min (width, (column + 1) * tileSize);

                    for (int row = 0; row < rows; row++)
                    {
                        var top = 0 == row ? 0 : row * tileSize - tileOverlap;
                        var bottom = Math.Min (height, (row + 1) * tileSize);

                        var rect = new Rect (new Point (left, top), new Point (right, bottom));
                        var tileName = TileName (row, column);
                        yield return new Pair<Rect, string> (rect, tileName);
                    }
                }
            }
        }
    }
}
