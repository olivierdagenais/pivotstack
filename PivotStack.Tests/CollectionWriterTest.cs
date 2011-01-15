using System.IO;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using SoftwareNinjas.Core;

namespace PivotStack.Tests
{
    [TestFixture]
    public class CollectionWriterTest
    {
        private static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            NewLineChars = "\n",
#if DEBUG
            Indent = true,
            IndentChars = "  ",
#else
            NewLineHandling = NewLineHandling.Entitize,
#endif
        };

        [Test]
        public void Typical ()
        {
            using (var ms = new MemoryStream ())
            using (var sw = new StreamWriter (ms))
            {
                XDocument doc;
                XmlNamespaceManager namespaceManager;
                using (var stream = AssemblyExtensions.OpenScopedResourceStream<Program> ("Template.cxml"))
                using (var reader = XmlReader.Create (stream))
                {
                    doc = XDocument.Load (reader);
                    namespaceManager = new XmlNamespaceManager (reader.NameTable);
                    namespaceManager.AddNamespace ("c", Namespaces.Collection.NamespaceName);
                }

                using (var cw = new CollectionWriter (ms, XmlWriterSettings, futureCw => 
                    {
                        futureCw.Flush ();
                        sw.Write (ProgramTest.ExpectedAnsweredAndAccepted);
                        sw.Flush ();
                    })
                )
                {
                    doc.Save (cw);
                }

                sw.Flush ();
                ProgramTest.AssertStreamsAreEqual<CollectionWriterTest> ("CollectionWithInjectedItems.cxml", ms);
            }
        }
    }
}
