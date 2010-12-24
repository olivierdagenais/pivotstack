using System.Drawing;
using NUnit.Framework;

namespace PivotStack.Tests
{
    [TestFixture]
    public class MortonLayoutTest
    {
        [Test]
        public void Decode_BaseCase ()
        {
            Assert.AreEqual (new Point (0, 0), MortonLayout.Decode (0));
            Assert.AreEqual (new Point (1, 0), MortonLayout.Decode (1));
            Assert.AreEqual (new Point (0, 1), MortonLayout.Decode (2));
            Assert.AreEqual (new Point (1, 1), MortonLayout.Decode (3));
        }

        [Test]
        public void Decode_Typical ()
        {
            Assert.AreEqual (new Point (3, 0), MortonLayout.Decode (5));
            Assert.AreEqual (new Point (6, 7), MortonLayout.Decode (62));
            Assert.AreEqual (new Point (0, 4), MortonLayout.Decode (32));
        }

        [Test]
        public void Decode_Edge ()
        {
            Assert.AreEqual (new Point (65535, 32767), MortonLayout.Decode (2147483647));
        }

        [Test]
        public void Decode_PowersOfFourMinusOneMapToPowersOfTwoMinusOne()
        {
            Assert.AreEqual (new Point (    1,     1), MortonLayout.Decode (         3));
            Assert.AreEqual (new Point (    3,     3), MortonLayout.Decode (        15));
            Assert.AreEqual (new Point (    7,     7), MortonLayout.Decode (        63));
            Assert.AreEqual (new Point (   15,    15), MortonLayout.Decode (       255));
            Assert.AreEqual (new Point (   31,    31), MortonLayout.Decode (      1023));
            Assert.AreEqual (new Point (   63,    63), MortonLayout.Decode (      4095));
            Assert.AreEqual (new Point (  127,   127), MortonLayout.Decode (     16383));
            Assert.AreEqual (new Point (  255,   255), MortonLayout.Decode (     65535));
            Assert.AreEqual (new Point (  511,   511), MortonLayout.Decode (    262143));
            Assert.AreEqual (new Point ( 1023,  1023), MortonLayout.Decode (   1048575));
            Assert.AreEqual (new Point ( 2047,  2047), MortonLayout.Decode (   4194303));
            Assert.AreEqual (new Point ( 4095,  4095), MortonLayout.Decode (  16777215));
            Assert.AreEqual (new Point ( 8191,  8191), MortonLayout.Decode (  67108863));
            Assert.AreEqual (new Point (16383, 16383), MortonLayout.Decode ( 268435455));
            Assert.AreEqual (new Point (32767, 32767), MortonLayout.Decode (1073741823));
        }
    }
}
