using System.IO;
using System.Xml;

namespace PivotStack
{
    internal class ItemWriter : InterceptingXmlWriter
    {
        private readonly bool _shouldEntitizeNewLines;
        private bool _isEntitizingNewLines;

        public ItemWriter (Stream output, XmlWriterSettings settings) : base (output, settings)
        {
            _shouldEntitizeNewLines = NewLineHandling.Entitize == settings.NewLineHandling;
            _isEntitizingNewLines = false;
        }

        public override void WriteStartElement (string prefix, string localName, string ns)
        {
            _isEntitizingNewLines = "Description" == localName && _shouldEntitizeNewLines;
            base.WriteStartElement (prefix, localName, ns);
        }

        public override void WriteString (string text)
        {
            if (_isEntitizingNewLines)
            {
                foreach (var c in text)
                {
                    if ('\n' == c)
                    {
                        base.WriteRaw ("&#xA;");
                    }
                    else
                    {
                        base.WriteString (c.ToString ());
                    }
                }
            }
            else
            {
                base.WriteString (text);
            }
        }

        public override void WriteEndDocument ()
        {
            if (_isEntitizingNewLines)
            {
                _isEntitizingNewLines = false;
            }
            base.WriteEndDocument ();
        }

        public override void WriteFullEndElement ()
        {
            if (_isEntitizingNewLines)
            {
                _isEntitizingNewLines = false;
            }
            base.WriteFullEndElement ();
        }
    }
}
