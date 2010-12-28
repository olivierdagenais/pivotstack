using NUnit.Framework;
using SoftwareNinjas.Core;
using SoftwareNinjas.Core.Test;

namespace PivotStack.Tests
{
    [TestFixture]
    public class ImageCollectionTileTest
    {
        [Test]
        public void Equals ()
        {
            var x = new ImageCollectionTile (2, 3, new[]
                { 
                    new Pair<int, int> (0, 0),
                    new Pair<int, int> (1, 1),
                }
            );
            var y = new ImageCollectionTile (2, 3, new[]
                { 
                    new Pair<int, int> (0, 0),
                    new Pair<int, int> (1, 1),
                }
            );
            var z = new ImageCollectionTile (2, 3, new[]
                { 
                    new Pair<int, int> (0, 0),
                    new Pair<int, int> (1, 1),
                }
            );
            var a = new ImageCollectionTile (2, 3, new[]
                { 
                    new Pair<int, int> (1, 1),
                    new Pair<int, int> (0, 0),
                }
            );
            var tester = new EqualsTester<ImageCollectionTile> (x, y, z, a);
            tester.Run ();
        }

        [Test]
        public new void ToString ()
        {
            var x = new ImageCollectionTile (2, 3, new[]
                { 
                    new Pair<int, int> (0, 0),
                    new Pair<int, int> (1, 1),
                }
            );
            Assert.AreEqual ("3_2 with 2 tiles, morton 0-1", x.ToString ());
        }

    }
}
