using System.Collections;
using System.Drawing;

namespace PivotStack
{
    public class MortonLayout
    {
        private const int NumberOfBits = 32;

        /// <summary>
        /// Decodes the specified <paramref name="index"/> into its corresponding column and row in a "Morton Layout".
        /// </summary>
        /// 
        /// <param name="index">
        /// The integer to convert into a "Morton Layout" location.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Point"/> representing the column (<see cref="Point.X"/>) and row (<see cref="Point.Y"/>)
        /// of the specified <paramref name="index"/> in a "Morton Layout".
        /// </returns>
        /// 
        /// <seealso href="http://msdn.microsoft.com/en-us/library/cc645077%28VS.95%29.aspx#Collections">
        /// Deep Zoom File Format Overview
        /// </seealso>
        internal static Point Decode(int index)
        {
            var bits = new BitArray (new[] {index});
            int x = 0, y = 0;
            for (int o = 1, powerOfTwo = 1; o < NumberOfBits; o += 2, powerOfTwo <<= 1)
            {
                if (bits[o - 1])
                {
                    x += powerOfTwo;
                }
                if (bits[o])
                {
                    y += powerOfTwo;
                }
            }
            var result = new Point (x, y);
            return result;
        }
    }
}
