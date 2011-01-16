using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

using SoftwareNinjas.Core;

using Tile = SoftwareNinjas.Core.Pair<System.Drawing.Rectangle, string>;

namespace PivotStack
{
    public class DeepZoomImage
    {
        internal const string TileNameTemplate = "{0}_{1}";
        internal static readonly string TileZeroZero = "0_0";
        internal static readonly Size OneByOne = new Size (1, 1);

        private readonly Settings _settings;

        private readonly int _maximumLevel;

        public DeepZoomImage(Settings settings)
        {
            _settings = settings;

            _maximumLevel = DetermineMaximumLevel (settings.ItemImageSize);
        }

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

        internal static IEnumerable<Pair<Bitmap,string>> Slice (Bitmap source, IEnumerable<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var rect = tile.First;
                var targetWidth = rect.Width;
                var targetHeight = rect.Height;
                using (var targetImage = new Bitmap (targetWidth, targetHeight))
                using (var graphics = Graphics.FromImage (targetImage))
                {
                    graphics.InterpolationMode = InterpolationMode.Default;
                    var destRect = new Rectangle(0, 0, targetWidth, targetHeight);
                    graphics.DrawImage (
                        source,
                        destRect,
                        rect.Left,
                        rect.Top,
                        targetWidth,
                        targetHeight,
                        GraphicsUnit.Pixel
                    );
                    yield return new Pair<Bitmap, string> (targetImage, tile.Second);
                }
            }
        }

        public void Slice(Bitmap source, IEnumerable<Tile> tiles, Func<string, Stream> streamGenerator)
        {
            var slices = Slice (source, tiles);
            foreach (var pair in slices)
            {
                var targetImage = pair.First;
                var streamName = pair.Second;
                var stream = streamGenerator (streamName);
                targetImage.Save (stream, _settings.PostImageEncoding);
            }
        }

        public void SlicePostImage(int postId)
        {
            var extension = _settings.PostImageEncoding.GetName ();
            var relativeBinnedImageFolder = Post.ComputeBinnedPath (postId, null, _settings.FileNameIdFormat) + "_files";
            var absoluteBinnedImageFolder = Path.Combine (_settings.AbsoluteWorkingFolder, relativeBinnedImageFolder);
            var absoluteBinnedOutputImageFolder = Path.Combine (_settings.AbsoluteOutputFolder, relativeBinnedImageFolder);

            for (var level = _maximumLevel; level >= 0; level--)
            {
                var levelName = Convert.ToString (level, 10);
                var targetSize = ComputeLevelSize (_settings.ItemImageSize, level);
                var tileFiles = new List<Stream> ();
                var inputLevelImageFile = Path.ChangeExtension (levelName, extension);
                var inputLevelImagePath = Path.Combine (absoluteBinnedImageFolder, inputLevelImageFile);
                var outputLevelFolder = Path.Combine (absoluteBinnedOutputImageFolder, levelName);
                Directory.CreateDirectory (outputLevelFolder);

                var tiles = ComputeTiles (targetSize);
                using (var inputStream =
                    new FileStream (inputLevelImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var levelBitmap = new Bitmap (inputStream))
                {
                    Slice (levelBitmap, tiles, tileName =>
                        {
                            var tileFileName = Path.ChangeExtension (tileName, extension);
                            var tilePath = Path.Combine (outputLevelFolder, tileFileName);
                            var stream =
                                new FileStream (tilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                            tileFiles.Add (stream);
                            return stream;
                        }
                    );
                }
                foreach (var stream in tileFiles)
                {
                    stream.Close ();
                }
            }
        }

        public IEnumerable<Tile> ComputeTiles(Size levelSize)
        {
            double width = levelSize.Width;
            double height = levelSize.Height;
            var maxDimension = Math.Max (width, height);
            if (maxDimension <= _settings.TileSize)
            {
                var pair = new Tile (CreateRectangle (levelSize), TileZeroZero);
                yield return pair;
            }
            else
            {
                var columns = (int) Math.Ceiling (width / _settings.TileSize);
                var rows = (int) Math.Ceiling (height / _settings.TileSize);
                var tileOffsetMultiplier = _settings.TileSize + _settings.TileOverlap - 1;

                for (int column = 0; column < columns; column++)
                {
                    var left = 0 == column ? 0 : column * _settings.TileSize - _settings.TileOverlap;
                    var right = Math.Min ((int) width - 1, (column + 1) * tileOffsetMultiplier);

                    for (int row = 0; row < rows; row++)
                    {
                        var top = 0 == row ? 0 : row * _settings.TileSize - _settings.TileOverlap;
                        var bottom = Math.Min ((int) height - 1, (row + 1) * tileOffsetMultiplier);

                        var rect = CreateRectangle (new Point (left, top), new Point (right, bottom));
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

        internal static Rectangle CreateRectangle(Size size)
        {
            return new Rectangle (0, 0, size.Width, size.Height);
        }

        internal static Rectangle CreateRectangle(Point point1, Point point2)
        {
            return new Rectangle (point1.X, point1.Y, point2.X - point1.X + 1, point2.Y - point1.Y + 1);
        }

        internal static void GeneratePostImageResizes (Bitmap sourceBitmap, Size size, int maximumLevel, Action<int, Bitmap> saveAction)
        {
            for (var level = maximumLevel; level >= 0; level--)
            {
                var targetSize = ComputeLevelSize (size, level);
                using (var resizedBitmap = Resize (sourceBitmap, targetSize.Width, targetSize.Height))
                {
                    saveAction (level, resizedBitmap);
                }
            }
        }
    }
}
