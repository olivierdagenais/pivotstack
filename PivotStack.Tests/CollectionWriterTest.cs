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
                    namespaceManager.AddNamespace ("c", Program.CollectionNamespace.NamespaceName);
                }

                using (var cw = new CollectionWriter (ms, Program.WriterSettings, futureCw => 
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
