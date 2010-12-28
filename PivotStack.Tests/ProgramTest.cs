using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Markup;

using NUnit.Framework;
using EnumerableExtensions = SoftwareNinjas.Core.Test.EnumerableExtensions;
using SoftwareNinjas.Core;

namespace PivotStack.Tests
{
    [TestFixture]
    public class ProgramTest
    {
        internal const string ExpectedAnsweredAndAccepted = @"
    <Item Id=""3232"" Href=""3232"" Name=""What are the best Excel tips?"">
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
        private readonly Page _testTemplate;

        public ProgramTest()
        {
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<ProgramTest> ("TestTemplate.xaml"))
            {
                _testTemplate = (Page) XamlReader.Load (stream);
                _testTemplate.Width = 800;
                _testTemplate.Height = 400;
            }
        }

        [Test]
        public void ImagePost_AnsweredAndAccepted ()
        {
            TestImagePost ("AnsweredAndAccepted.png", PostTest.AnsweredAndAccepted);
        }

        private void TestImagePost (string expectedFileName, Post inputPost)
        {
            using (var outputStream = new MemoryStream())
            {
                Program.ImagePost (inputPost, _testTemplate, ImageFormat.Png, outputStream);
                AssertStreamsAreEqual<ProgramTest> (expectedFileName, outputStream);
            }
        }

        public void GenerateImageManifest_Typical()
        {
            const string expectedXml = @"
<Image xmlns='http://schemas.microsoft.com/deepzoom/2009' TileSize='254' Overlap='1' Format='png'>
  <Size Width='800' Height='400' />
</Image>";
            var expectedImageNode = XElement.Parse (expectedXml);

            var actualImageNode = Program.GenerateImageManifest (254, 1, "png", 800, 400);

            Assert.AreEqual (expectedImageNode.ToString (), actualImageNode.ToString ());
        }

        // TODO: Move to re-usable class library
        internal static void AssertStreamsAreEqual<T>(string expectedResourceFileName, MemoryStream actualStream)
        {
            using (var expectedStream = AssemblyExtensions.OpenScopedResourceStream<T> (expectedResourceFileName))
            {
                var expectedBytes = expectedStream.EnumerateBytes ();

                actualStream.Seek (0, SeekOrigin.Begin);
                var actualBytes = actualStream.EnumerateBytes ();

                try
                {
                    EnumerableExtensions.EnumerateSame (expectedBytes, actualBytes);
                }
                catch (AssertionException)
                {
                    actualStream.Seek (0, SeekOrigin.Begin);
                    using (var fileStream = new FileStream (expectedResourceFileName, FileMode.Create, FileAccess.Write))
                    {
                        actualStream.WriteTo (fileStream);
                    }
                    throw;
                }
            }            
        }

        private static void TestPivotizePost (string expectedXml, ICollection values)
        {
            // arrange
            var row = new ArrayList (values);
            var post = Post.LoadFromRow (row);

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
            TestPivotizePost (ExpectedAnsweredAndAccepted, PostTest.AnsweredAndAccepted);
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
    <Item Id=""3232"" Href=""3232"" Name=""What are the best Excel tips?"">
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
    <Item Id=""3232"" Href=""3232"" Name=""What are the best Excel tips?"">
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
