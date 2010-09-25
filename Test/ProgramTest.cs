using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Markup;

using NUnit.Framework;
using EnumerableExtensions = SoftwareNinjas.Core.Test.EnumerableExtensions;
using SoftwareNinjas.Core;

namespace PivotStack.Test
{
    [TestFixture]
    public class ProgramTest
    {
        private readonly Page _testTemplate;

        public ProgramTest()
        {
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<ProgramTest> ("TestTemplate.xaml"))
            {
                _testTemplate = (Page) XamlReader.Load (stream);
            }
        }

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
            Assert.AreEqual (expected, Program.CleanHtml (html));
        }

        [Test]
        public void FileNameToBinnedPath_LotsOfBins ()
        {
            Assert.AreEqual ("123/456/789/123456789ABC.png", Program.FileNameToBinnedPath ("123456789ABC.png", 3));
        }

        [Test]
        public void FileNameToBinnedPath_MultipleOfBinSize ()
        {
            Assert.AreEqual ("123/123456.png", Program.FileNameToBinnedPath ("123456.png", 3));
        }

        [Test]
        public void FileNameToBinnedPath_OneShortOfSecondBin ()
        {
            Assert.AreEqual ("12/12456.png", Program.FileNameToBinnedPath ("12456.png", 3));
        }

        [Test]
        public void FileNameToBinnedPath_OneInSecondBin ()
        {
            Assert.AreEqual ("1/1456.png", Program.FileNameToBinnedPath ("1456.png", 3));
        }

        [Test]
        public void FileNameToBinnedPath_One ()
        {
            Assert.AreEqual ("1.png", Program.FileNameToBinnedPath ("1.png", 3));
        }

        [Test]
        public void FileNameToBinnedPath_OneShortOfBin ()
        {
            Assert.AreEqual ("12.png", Program.FileNameToBinnedPath ("12.png", 3));
        }

        [Test]
        public void FileNameToBinnedPath_OneBin ()
        {
            Assert.AreEqual ("123.png", Program.FileNameToBinnedPath ("123.png", 3));
        }

        [Test]
        public void BreakUpStringReverse_LotsOfBins ()
        {
            var expected = new[] { "ABC", "789", "456", "123" };
            EnumerableExtensions.EnumerateSame (expected, Program.BreakUpStringReverse ("123456789ABC", 3));
        }

        [Test]
        public void BreakUpStringReverse_MultipleOfBinSize ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "456", "123" }, Program.BreakUpStringReverse ("123456", 3));
        }

        [Test]
        public void BreakUpStringReverse_OneShortOfSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "456", "12" }, Program.BreakUpStringReverse ("12456", 3));
        }

        [Test]
        public void BreakUpStringReverse_OneInSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "456", "1" }, Program.BreakUpStringReverse ("1456", 3));
        }

        [Test]
        public void BreakUpStringReverse_One ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "1" }, Program.BreakUpStringReverse ("1", 3));
        }

        [Test]
        public void BreakUpStringReverse_OneShortOfBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "12" }, Program.BreakUpStringReverse ("12", 3));
        }

        [Test]
        public void BreakUpStringReverse_OneBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "123" }, Program.BreakUpStringReverse ("123", 3));
        }

        [Test]
        public void BreakUpString_LotsOfBins ()
        {
            var expected = new[] { "123", "456", "789", "ABC" };
            EnumerableExtensions.EnumerateSame (expected, Program.BreakUpString ("123456789ABC", 3));
        }

        [Test]
        public void BreakUpString_MultipleOfBinSize ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "123", "456" }, Program.BreakUpString ("123456", 3));
        }

        [Test]
        public void BreakUpString_OneShortOfSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "12", "456" }, Program.BreakUpString ("12456", 3));
        }

        [Test]
        public void BreakUpString_OneInSecondBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "1", "456" }, Program.BreakUpString ("1456", 3));
        }

        [Test]
        public void BreakUpString_One ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "1" }, Program.BreakUpString ("1", 3));
        }

        [Test]
        public void BreakUpString_OneShortOfBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "12" }, Program.BreakUpString ("12", 3));
        }

        [Test]
        public void BreakUpString_OneBin ()
        {
            EnumerableExtensions.EnumerateSame (new[] { "123" }, Program.BreakUpString ("123", 3));
        }

        [Test]
        public void ImagePost_AnsweredAndAccepted ()
        {
            TestImagePost ("AnsweredAndAccepted.png", PostTest.AnsweredAndAccepted);
        }

        private void TestImagePost (string expectedFileName, Post inputPost)
        {
            using (var expectedStream = AssemblyExtensions.OpenScopedResourceStream<ProgramTest> (expectedFileName))
            using (var outputStream = new MemoryStream())
            {
                var expectedBytes = expectedStream.EnumerateBytes ();

                Program.ImagePost (inputPost, _testTemplate, BitmapEncoding.Png, outputStream);
                outputStream.Seek (0, SeekOrigin.Begin);
                try
                {
                    var actualBytes = outputStream.EnumerateBytes ();
                    EnumerableExtensions.EnumerateSame (expectedBytes, actualBytes);
                }
                catch (AssertionException)
                {
                    outputStream.Seek (0, SeekOrigin.Begin);
                    using (var fileStream = new FileStream (expectedFileName, FileMode.Create, FileAccess.Write))
                    {
                        outputStream.WriteTo (fileStream);
                    }
                    throw;
                }
            }
        }

        private static void TestParseTags (string input, params string[] expected)
        {
            var actual = Program.ParseTags (input);
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

        private static void TestPivotizePost (string expectedXml, ICollection values)
        {
            // arrange
            var row = new ArrayList (values);
            var post = Post.Load (row);

            // act and assert
            TestPivotizePost (expectedXml, post);
        }

        private static void TestPivotizePost (string expectedXml, Post post)
        {
            // arrange
            var expectedItemNode = XElement.Parse (expectedXml);

            // act
            var actualItemNode = Program.PivotizePost (post);

            // assert
            Assert.AreEqual (expectedItemNode.ToString (), actualItemNode.ToString ());
        }

        [Test]
        public void PivotizePost_AnsweredAndAccepted ()
        {
            const string expectedXml = @"
    <Item Id=""3232"" Href=""3232"" Name=""What are the best Excel tips?""
        xmlns=""http://schemas.microsoft.com/collection/metadata/2009"">
      <Description>What are your best tips/not so known features of excel?</Description>
      <Facets>
        <Facet Name=""Score""><Number Value=""7"" /></Facet>
        <Facet Name=""Views""><Number Value=""761"" /></Facet>
        <Facet Name=""Answers""><Number Value=""27"" /></Facet>
        <Facet Name=""Tagged""><String Value=""excel"" /><String Value=""tips-and-tricks"" /></Facet>
        <Facet Name=""Related Tags"">
            <Link Href=""excel.cxml"" Name=""excel"" />
            <Link Href=""tips-and-tricks.cxml"" Name=""tips-and-tricks"" />
        </Facet>
        <Facet Name=""Date asked""><DateTime Value=""2009-07-15T18:36:28"" /></Facet>
        <Facet Name=""Is answered?""><String Value=""yes"" /></Facet>
        <Facet Name=""Date first answered""><DateTime Value=""2009-07-15T18:41:08"" /></Facet>
        <Facet Name=""Date last answered""><DateTime Value=""2010-06-16T09:46:07"" /></Facet>
        <Facet Name=""Asker""><String Value=""Bob"" /></Facet>
        <Facet Name=""Has accepted answer?""><String Value=""yes"" /></Facet>
        <Facet Name=""Accepted Answer""><LongString Value=""My best advice for Excel..."" /></Facet>
        <Facet Name=""Top Answer""><LongString Value=""In-cell graphs, using REPT..."" /></Facet>
        <Facet Name=""Is favorite?""><String Value=""yes"" /></Facet>
        <Facet Name=""Favorites""><Number Value=""10"" /></Facet>
      </Facets>
    </Item>";
            TestPivotizePost (expectedXml, PostTest.AnsweredAndAccepted);
        }

        [Test]
        public void PivotizePost_Answered ()
        {
            var data = new OrderedDictionary
            {
                {"Id", 3232},
                {"Title", "What are the best Excel tips?"},
                {"Description", "What are your best tips/not so known features of excel?"},
                {"Score", 7},
                {"Views", 761},
                {"Answers", 27},
                {"Tagged", "<excel><tips-and-tricks>"},
                {"DateAsked", new DateTime(2009, 07, 15, 18, 36, 28)},
                {"DateFirstAnswered", new DateTime(2009, 07, 15, 18, 41, 08)},
                {"DateLastAnswered", new DateTime(2010, 06, 16, 09, 46, 07)},
                {"Asker", "Bob"},
                {"AcceptedAnswerId", DBNull.Value},
                {"AcceptedAnswer", DBNull.Value},
                {"TopAnswerId", 21231},
                {"TopAnswer", "In-cell graphs..."},
                {"Favorites", 10},
            };
            const string expectedXml = @"
    <Item Id=""3232"" Href=""3232"" Name=""What are the best Excel tips?""
        xmlns=""http://schemas.microsoft.com/collection/metadata/2009"">
      <Description>What are your best tips/not so known features of excel?</Description>
      <Facets>
        <Facet Name=""Score""><Number Value=""7"" /></Facet>
        <Facet Name=""Views""><Number Value=""761"" /></Facet>
        <Facet Name=""Answers""><Number Value=""27"" /></Facet>
        <Facet Name=""Tagged""><String Value=""excel"" /><String Value=""tips-and-tricks"" /></Facet>
        <Facet Name=""Related Tags"">
            <Link Href=""excel.cxml"" Name=""excel"" />
            <Link Href=""tips-and-tricks.cxml"" Name=""tips-and-tricks"" />
        </Facet>
        <Facet Name=""Date asked""><DateTime Value=""2009-07-15T18:36:28"" /></Facet>
        <Facet Name=""Is answered?""><String Value=""yes"" /></Facet>
        <Facet Name=""Date first answered""><DateTime Value=""2009-07-15T18:41:08"" /></Facet>
        <Facet Name=""Date last answered""><DateTime Value=""2010-06-16T09:46:07"" /></Facet>
        <Facet Name=""Asker""><String Value=""Bob"" /></Facet>
        <Facet Name=""Has accepted answer?""><String Value=""no"" /></Facet>
        <Facet Name=""Top Answer""><LongString Value=""In-cell graphs..."" /></Facet>
        <Facet Name=""Is favorite?""><String Value=""yes"" /></Facet>
        <Facet Name=""Favorites""><Number Value=""10"" /></Facet>
      </Facets>
    </Item>";
            TestPivotizePost (expectedXml, data.Values);
        }

        [Test]
        public void PivotizePost_Unanswered ()
        {
            var data = new OrderedDictionary
            {
                {"Id", 3232},
                {"Title", "What are the best Excel tips?"},
                {"Description", "What are your best tips/not so known features of excel?"},
                {"Score", 7},
                {"Views", 761},
                {"Answers", 0},
                {"Tagged", "<excel><tips-and-tricks>"},
                {"DateAsked", new DateTime(2009, 07, 15, 18, 36, 28)},
                {"DateFirstAnswered", DBNull.Value},
                {"DateLastAnswered", DBNull.Value},
                {"Asker", "Bob"},
                {"AcceptedAnswerId", DBNull.Value},
                {"AcceptedAnswer", DBNull.Value},
                {"TopAnswerId", DBNull.Value},
                {"TopAnswer", DBNull.Value},
                {"Favorites", 10},
            };
            const string expectedXml = @"
    <Item Id=""3232"" Href=""3232"" Name=""What are the best Excel tips?""
        xmlns=""http://schemas.microsoft.com/collection/metadata/2009"">
      <Description>What are your best tips/not so known features of excel?</Description>
      <Facets>
        <Facet Name=""Score""><Number Value=""7"" /></Facet>
        <Facet Name=""Views""><Number Value=""761"" /></Facet>
        <Facet Name=""Answers""><Number Value=""0"" /></Facet>
        <Facet Name=""Tagged""><String Value=""excel"" /><String Value=""tips-and-tricks"" /></Facet>
        <Facet Name=""Related Tags"">
            <Link Href=""excel.cxml"" Name=""excel"" />
            <Link Href=""tips-and-tricks.cxml"" Name=""tips-and-tricks"" />
        </Facet>
        <Facet Name=""Date asked""><DateTime Value=""2009-07-15T18:36:28"" /></Facet>
        <Facet Name=""Is answered?""><String Value=""no"" /></Facet>
        <Facet Name=""Asker""><String Value=""Bob"" /></Facet>
        <Facet Name=""Has accepted answer?""><String Value=""no"" /></Facet>
        <Facet Name=""Is favorite?""><String Value=""yes"" /></Facet>
        <Facet Name=""Favorites""><Number Value=""10"" /></Facet>
      </Facets>
    </Item>";
            TestPivotizePost (expectedXml, data.Values);
        }
    }
}
