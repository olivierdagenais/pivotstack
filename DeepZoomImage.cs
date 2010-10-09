using System;

namespace PivotStack
{
    public class DeepZoomImage
    {
        internal static readonly Size<int> OneByOne = Size.Create (1, 1);

        internal static Size<int> ComputeLevelSize(Size<int> originalSize, int levelNumber)
        {
            if (levelNumber < 0)
            {
                throw new ArgumentOutOfRangeException("levelNumber", levelNumber, "levelNumber must be >= 0");
            }

            Size<int> result;
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
                    result = Size.Create(width, height);
                }
            }

            return result;
        }
    }
}
