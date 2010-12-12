using System;
using System.Text;
using NUnit.Framework;
using EnumerableExtensions = SoftwareNinjas.Core.Test.EnumerableExtensions;

namespace PivotStack.Tests
{
    [TestFixture]
    public class StringExtensionsTest
    {
        [Test]
        public void CleanHtml ()
        {
            const string html = @"
<p>After reading <a href=""http://superuser.com/questions/2902/which-online-game-you-are-playing-in-your-free-time"">this question</a>, I was inspired to create this wiki on Nethack tips and tricks.</p>

<p>There must be many people in the SU community that play nethack, so I ask you, what are some of your best strategies for nethack?</p>

<p>Also if you want, share some of your best stories that have happened while playing the game.</p>

<p>For those you out there that don't know what nethack is: <a href=""http://nethack.wikia.com/wiki/Main%5FPage"" rel=""nofollow"" title=""Nethack Wiki"">please inform</a> <a href=""http://en.wikipedia.org/wiki/NetHack"" rel=""nofollow"" title=""Wiki on Nethack"">your selves</a></p>";
            const string expected = @"
After reading this question, I was inspired to create this wiki on Nethack tips and tricks.

There must be many people in the SU community that play nethack, so I ask you, what are some of your best strategies for nethack?

Also if you want, share some of your best stories that have happened while playing the game.

For those you out there that don't know what nethack is: please inform your selves";
            Assert.AreEqual (expected, html.CleanHtml ());
        }

        [Test]
        public void ToBinnedPath_LotsOfBins ()
        {
            Assert.AreEqual ("123/456/789/123456789ABC.png", "123456789ABC.png".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_MultipleOfBinSize ()
        {
            Assert.AreEqual ("123/123456.png", "123456.png".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_OneShortOfSecondBin ()
        {
            Assert.AreEqual ("12/12456.png", "12456.png".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_OneInSecondBin ()
        {
            Assert.AreEqual ("1/1456.png", "1456.png".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_One ()
        {
            Assert.AreEqual ("1.png", "1.png".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_OneShortOfBin ()
        {
            Assert.AreEqual ("12.png", "12.png".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_OneBin ()
        {
            Assert.AreEqual ("123.png", "123.png".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_ReservedWord ()
        {
            Assert.AreEqual ("t/ele/_con/ver/teleconverter.cxml", "teleconverter.cxml".ToBinnedPath (3));
        }

        [Test]
        public void ToBinnedPath_ReservedCharacters ()
        {
            var bytes = new byte[] {0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC,};
            var sb = new StringBuilder (bytes.Length);
            foreach (var b in bytes)
            {
                var c = Convert.ToChar (b);
                sb.Append (c);
            }
            sb.Append (".cxml");
            var input = sb.ToString ();

            var actual = input.ToBinnedPath (3);

            Assert.AreEqual ("%0/%1%2%3/%4%5%6/%7%8%9/%0%1%2%3%4%5%6%7%8%9%a%b%c.cxml", actual);
        }

        [Test]
        public void SanitizeName_NoChanges ()
        {
            Assert.AreEqual ("teleconverter.cxml", StringExtensions.SanitizeName ("teleconverter.cxml"));
        }

        [Test]
        public void SanitizeName_ReservedDeviceName ()
        {
            Assert.AreEqual ("_con", StringExtensions.SanitizeName ("con"));
        }

        [Test]
        public void SanitizeName_Percent ()
        {
            Assert.AreEqual ("%25", StringExtensions.SanitizeName ("%"));
        }


        [Test]
        public void BinUpReverse_LotsOfBins ()
        {
            var expected = new[] { "ABC", "789", "456", "123" };
            EnumerableExtensions.EnumerateSame (expected, "123456789ABC".BinUpReverse (3));
        }

        [Test]
        public void BinUpReverse_MultipleOfBinSize ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "456", "123" }, "123456".BinUpReverse (3));
        }

        [Test]
        public void BinUpReverse_OneShortOfSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "456", "12" }, "12456".BinUpReverse (3));
        }

        [Test]
        public void BinUpReverse_OneInSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "456", "1" }, "1456".BinUpReverse (3));
        }

        [Test]
        public void BinUpReverse_One ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "1" }, "1".BinUpReverse (3));
        }

        [Test]
        public void BinUpReverse_OneShortOfBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "12" }, "12".BinUpReverse (3));
        }

        [Test]
        public void BinUpReverse_OneBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "123" }, "123".BinUpReverse (3));
        }

        [Test]
        public void BinUp_LotsOfBins ()
        {
            var expected = new[] { "123", "456", "789", "ABC" };
            EnumerableExtensions.EnumerateSame (expected, "123456789ABC".BinUp (3));
        }

        [Test]
        public void BinUp_MultipleOfBinSize ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "123", "456" }, "123456".BinUp (3));
        }

        [Test]
        public void BinUp_OneShortOfSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "12", "456" }, "12456".BinUp (3));
        }

        [Test]
        public void BinUp_OneInSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "1", "456" }, "1456".BinUp (3));
        }

        [Test]
        public void BinUp_One ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "1" }, "1".BinUp (3));
        }

        [Test]
        public void BinUp_OneShortOfBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "12" }, "12".BinUp (3));
        }

        [Test]
        public void BinUp_OneBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "123" }, "123".BinUp (3));
        }

        private static void TestParseTags (string input, params string[] expected)
        {
            var actual = input.ParseTags ();
            EnumerableExtensions.EnumerateSame (expected, actual);
        }

        [Test]
        public void ParseTags_One ()
        {
            TestParseTags ("<tips-and-tricks>", "tips-and-tricks");
        }

        [Test]
        public void ParseTags_Two ()
        {
            TestParseTags ("<windows-xp><copy-paste>", "windows-xp", "copy-paste");
        }

        [Test]
        public void ParseTags_MoreThanLettersAndHyphens ()
        {
            TestParseTags ("<ie8><ubuntu-10.04-lts><visual-c++>", "ie8", "ubuntu-10.04-lts", "visual-c++");
        }
    }
}
