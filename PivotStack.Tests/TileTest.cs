using System.Drawing;
using NUnit.Framework;

namespace PivotStack.Tests
{
    [TestFixture]
    public class TileTest
    {
        private static void AssertAreEqual (System.Windows.Rect expected, Rectangle actual)
        {
            Assert.AreEqual ((int) expected.X,      actual.X);
            Assert.AreEqual ((int) expected.Y,      actual.Y);
            Assert.AreEqual ((int) expected.Width,  actual.Width);
            Assert.AreEqual ((int) expected.Height, actual.Height);

            Assert.AreEqual ((int) expected.Top,    actual.Top);
            Assert.AreEqual ((int) expected.Bottom, actual.Bottom);
            Assert.AreEqual ((int) expected.Left,   actual.Left);
            Assert.AreEqual ((int) expected.Right,  actual.Right);
        }

        private static void CheckRectangleBySize(int width, int height)
        {
            var expected = new System.Windows.Rect (new System.Windows.Size (width, height));
            var actual = Tile.CreateRectangle (new Size (width, height));
            AssertAreEqual (expected, actual);
        }

        [Test]
        public void CreateRectangle_WithSize_EdgeCases()
        {
            CheckRectangleBySize (         0,          0);
            CheckRectangleBySize (         1,          1);
            CheckRectangleBySize (       255,        255);
            CheckRectangleBySize (     32768,      32768);
            CheckRectangleBySize (2147483647, 2147483647);
        }

        private static void CheckRectangleWithTwoPoints(int x1, int y1, int x2, int y2)
        {
            // The constructor below is documented as:
            // "Initializes a new instance of the Rect structure that is
            // exactly large enough to contain the two specified points."
            var expected = new System.Windows.Rect
            (
                new System.Windows.Point (x1, y1),
                new System.Windows.Point (x2 + 1, y2 + 1)
            );
            var actual = Tile.CreateRectangle (new Point (x1, y1), new Point (x2, y2));
            AssertAreEqual (expected, actual);
        }

        [Test]
        public void CreateRectangle_WithTwoPoints_Typical()
        {
            CheckRectangleWithTwoPoints (  0,   0, 254, 254);
            CheckRectangleWithTwoPoints (  0, 253, 254, 374);
            CheckRectangleWithTwoPoints (253,   0, 299, 254);
            CheckRectangleWithTwoPoints (253, 253, 299, 374);
        }

        [Test]
        public void CreateRectangle_WithTwoPoints_EdgeCases()
        {
            CheckRectangleWithTwoPoints (  0,   0,   0,   0);
            CheckRectangleWithTwoPoints (  0,   0,   1,   1);
            CheckRectangleWithTwoPoints (  1,   1,   1,   1);
            CheckRectangleWithTwoPoints (  2,   2,   2,   2);
            CheckRectangleWithTwoPoints (  2,   2,   4,   4);
            CheckRectangleWithTwoPoints (  0,   0, 255, 255);
            CheckRectangleWithTwoPoints (  2,   2,   2,   2);
        }
    }
}
