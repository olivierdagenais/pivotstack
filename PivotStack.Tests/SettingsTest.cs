using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace PivotStack.Tests
{
    [TestFixture]
    public class SettingsTest
    {
        private static readonly XmlReaderSettings XmlReaderSettings = new XmlReaderSettings
        {
#if DEBUG
            IgnoreWhitespace = false,
#else
            IgnoreWhitespace = true,
#endif
        };

        [Test]
        public void MaximumNumberOfDigits_9 ()
        {
            var settings = new Settings
            {
                HighestId = 9,
            };
            Assert.AreEqual (1, settings.MaximumNumberOfDigits);
        }

        [Test]
        public void MaximumNumberOfDigits_10()
        {
            var settings = new Settings
            {
                HighestId = 10,
            };
            Assert.AreEqual (2, settings.MaximumNumberOfDigits);
        }

        [Test]
        public void MaximumNumberOfDigits_936 ()
        {
            var settings = new Settings
            {
                HighestId = 936,
            };
            Assert.AreEqual (3, settings.MaximumNumberOfDigits);
        }

        [Test]
        public void MaximumNumberOfDigits_1000 ()
        {
            var settings = new Settings
            {
                HighestId = 1000,
            };
            Assert.AreEqual (4, settings.MaximumNumberOfDigits);
        }

        [Test]
        public void GenerateImageManifest_Typical ()
        {
            const string expectedXml = @"
<Image xmlns='http://schemas.microsoft.com/deepzoom/2009' TileSize='254' Overlap='1' Format='png'>
  <Size Width='800' Height='400' />
</Image>";
            var expectedImageNode = XElement.Parse (expectedXml);

            var settings = new Settings
            {
                TileSize = 254,
                TileOverlap = 1,
                PostImageEncoding = ImageFormat.Png,
                ItemImageSize = new Size(800, 400),
                XmlReaderSettings = XmlReaderSettings,
            };
            var actualImageNode = settings.GenerateImageManifest ();

            Assert.AreEqual (expectedImageNode.ToString (), actualImageNode.ToString ());
        }
    }
}
