using NUnit.Framework;

namespace PivotStack.Tests
{
    [TestFixture]
    public class SettingsTest
    {
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
    }
}
