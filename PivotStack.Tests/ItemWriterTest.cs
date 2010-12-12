using System.IO;
using System.Xml;
using NUnit.Framework;

namespace PivotStack.Tests
{
    [TestFixture]
    public class ItemWriterTest
    {
        const string InputXml =
            "<Item Id=\"1\" Href=\"1\" Name=\"What is the best way to micro-adjust a lens?\">\n"
            + "  <Description>I have a Canon 7D with a 50mm F1.4 &amp; I think the auto-focus of the lens is off."
            + "\n\n"
            + "What is the best tutorial on how to do this reliably?"
            + "\n"
            + "</Description>\n"
            + "  <Facets>\n"
            + "    <Facet Name=\"Score\">\n"
            + "      <Number Value=\"9\" />\n"
            + "    </Facet>\n"
            + "  </Facets>\n"
            + "</Item>";

        [Test]
        public void Write_Optimized ()
        {
            const string expected =
                "<Item Id=\"1\" Href=\"1\" Name=\"What is the best way to micro-adjust a lens?\">"
                + "<Description>I have a Canon 7D with a 50mm F1.4 &amp; I think the auto-focus of the lens is off."
                + "&#xA;&#xA;"
                + "What is the best tutorial on how to do this reliably?"
                + "&#xA;"
                + "</Description>"
                + "<Facets><Facet Name=\"Score\"><Number Value=\"9\" /></Facet>"
                + "</Facets></Item>";


            var optimizedSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Entitize,
            };
            VerifyItemWriter (InputXml, expected, optimizedSettings);
        }

        [Test]
        public void Write_Debug ()
        {
            var debugSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
            };
            VerifyItemWriter (InputXml, InputXml, debugSettings);
        }

        private static void VerifyItemWriter (string inputXml, string expected, XmlWriterSettings writerSettings)
        {
            using (var ms = new MemoryStream())
            {
                var doc = new XmlDocument ();
                doc.LoadXml (inputXml);
                var writer = new ItemWriter (ms, writerSettings);
                doc.WriteTo (writer);
                writer.Flush ();

                ms.Seek (0, SeekOrigin.Begin);
                var sr = new StreamReader (ms);
                var actual = sr.ReadToEnd ();

                Assert.AreEqual (expected, actual);
            }
        }
    }
}
