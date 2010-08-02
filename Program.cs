using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using PivotStack.Properties;
using SoftwareNinjas.Core;

namespace PivotStack
{
    internal enum FacetType
    {
        String,
        LongString,
        Number,
        DateTime,
        Link,
    }

    public class Program
    {
        // TODO: parameterize
        internal const string SiteDomain = "superuser.com";

        internal static readonly XNamespace CollectionNamespace
            = "http://schemas.microsoft.com/collection/metadata/2009";
        internal static readonly XNamespace PivotNamespace 
            = "http://schemas.microsoft.com/livelabs/pivot/collection/2009";
        internal static readonly string SelectTags = LoadCommandText ("select-tags.sql");
        internal static readonly string SelectPostsByTag = LoadCommandText ("select-posts-by-tag.sql");
        internal static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
        {
#if DEBUG
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
#endif
        };
        internal static readonly Regex TagsRegex
            = new Regex (@"<(?<tag>[^>]+)>", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal static IEnumerable<string> ParseTags (string tagsColumn)
        {
            var matches = TagsRegex.Matches (tagsColumn);
            foreach (Match match in matches)
            {
                var tag = match.Groups["tag"].Value;
                yield return tag;
            }
        }

        public static int Main (string[] args)
        {
            using (var conn = new SqlConnection(Settings.Default.DatabaseConnectionString))
            {
                conn.Open ();
                var tags = EnumerateTags (conn);
                //foreach (var tag in tags)
                var tag = "tips-and-tricks";
                {
                    using (var outputStream 
                        = new FileStream (tag + ".cxml", FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        var parameters = new Dictionary<string, object> { {"@tag", tag} };
                        var posts = EnumerateRecords (conn, SelectPostsByTag, parameters);
                        PivotizeTag (tag, posts, outputStream);
                    }
                }
            }
            return 0;
        }

        internal static void PivotizeTag (string tag, IEnumerable<object[]> posts, Stream destination)
        {
            XDocument doc;
            XmlNamespaceManager namespaceManager;
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<Program> ("Template.cxml"))
            using (var reader = XmlReader.Create(stream))
            {
                doc = XDocument.Load (reader);
                namespaceManager = new XmlNamespaceManager(reader.NameTable);
                namespaceManager.AddNamespace("c", CollectionNamespace.NamespaceName);
                namespaceManager.AddNamespace("p", PivotNamespace.NamespaceName);
            }
            var collectionNode = doc.Root;
            collectionNode.SetAttributeValue ("Name", "Tagged Questions: {0}".FormatInvariant(tag));
            // TODO: do we want to strip hyphens from tag for AdditionalSearchText?
            collectionNode.SetAttributeValue (PivotNamespace + "AdditionalSearchText", tag);

            var itemsNode = collectionNode.XPathSelectElement ("c:Items", namespaceManager);
            itemsNode.SetAttributeValue ("HrefBase", "http://{0}/questions/".FormatInvariant (SiteDomain));
            foreach (var row in posts)
            {
                var element = PivotizePost (row);
                itemsNode.Add (element);
            }
            using (var writer = XmlWriter.Create (destination, WriterSettings))
            {
                doc.Save (writer);
            }
        }

        internal static XElement PivotizePost (IList row)
        {
            #region <Item Id="3232" Href="3232" Name="What are the best Excel tips?">
            var itemNode = new XElement (CollectionNamespace + "Item");

            var id = (int)row[0];
            itemNode.SetAttributeValue ("Id", id);
            itemNode.SetAttributeValue ("Href", id);

            var name = (string)row[1];
            itemNode.SetAttributeValue ("Name", name);

            #region <Description>What are your best tips/not so known features of excel?</Description>
            var description = (string)row[2];
            // TODO: strip HTML?
            var descriptionNode = new XElement(CollectionNamespace + "Description", description);
            itemNode.Add (descriptionNode);
            #endregion

            #region <Facets>
            var facetsNode = new XElement(CollectionNamespace + "Facets");

            #region <Facet Name="Score"><Number Value="7" /></Facet>
            var score = (int)row[3];
            AddFacet (facetsNode, FacetType.Number, "Score", score);
            #endregion

            #region <Facet Name="Views"><Number Value="761" /></Facet>
            var views = (int)row[4];
            AddFacet (facetsNode, FacetType.Number, "Views", views);
            #endregion

            #region <Facet Name="Answers"><Number Value="27" /></Facet>
            var answers = (int)row[5];
            AddFacet (facetsNode, FacetType.Number, "Answers", answers);
            #endregion

            #region <Facet Name="Tagged"><String Value="excel" /><String Value="tips-and-tricks" /></Facet>
            if (row[6] != DBNull.Value)
            {
                var rawTags = (string)row[6];
                var tags = ParseTags (rawTags);
                // TODO: make these of type FacetType.Link
                AddFacet (facetsNode, FacetType.String, "Tagged", tags.Cast<object>());
            }
            #endregion

            #region <Facet Name="Date asked"><DateTime Value="2009-07-15T18:41:08" /></Facet>
            var dateAsked = (DateTime)row[7];
            AddFacet (facetsNode, FacetType.DateTime, "Date asked", dateAsked.ToString ("s"));
            #endregion

            DateTime? dateFirstAnswered = ( row[8] != DBNull.Value ) ? (DateTime?)row[8] : null;
            #region <Facet Name="Is answered?"><String Value="yes" /></Facet>
            AddFacet (facetsNode, FacetType.String, "Is answered?", YesNo (dateFirstAnswered.HasValue));
            #endregion

            #region <Facet Name="Date first answered"><DateTime Value="2009-07-15T18:41:08" /></Facet>
            if (dateFirstAnswered.HasValue)
            {
                AddFacet (facetsNode, FacetType.DateTime, "Date first answered", dateFirstAnswered.Value.ToString ("s"));
            }
            #endregion

            #region <Facet Name="Date last answered"><DateTime Value="2010-06-16T09:46:07" /></Facet>
            DateTime? dateLastAnswered = ( row[9] != DBNull.Value ) ? (DateTime?)row[9] : null;
            if (dateLastAnswered.HasValue)
            {
                AddFacet (facetsNode, FacetType.DateTime, "Date last answered", dateLastAnswered.Value.ToString ("s"));
            }
            #endregion

            #region <Facet Name="Asker"><String Value="Bob" /></Facet>
            var asker = (string)row[10];
            if (asker != null)
            {
                AddFacet (facetsNode, FacetType.String, "Asker", asker);
            }
            #endregion

            #region <Facet Name="Has accepted answer?"><String Value="yes" /></Facet>
            int? acceptedAnswerId = ( row[11] != DBNull.Value ) ? (int?)row[11] : null;
            AddFacet (facetsNode, FacetType.String, "Has accepted answer?", YesNo (acceptedAnswerId.HasValue));
            #endregion

            #region <Facet Name="Accepted Answer"><String Value="My best advice for Excel..." /></Facet>
            string acceptedAnswer = ( row[12] != DBNull.Value ) ? (string)row[12] : null;
            if (acceptedAnswer != null)
            {
                // TODO: strip HTML?
                AddFacet (facetsNode, FacetType.LongString, "Accepted Answer", acceptedAnswer);
                // TODO: link to accepted answer
            }
            #endregion

            #region <Facet Name="Top Answer"><String Value="In-cell graphs..." /></Facet>
            int? topAnswerId = ( row[13] != DBNull.Value ) ? (int?)row[13] : null;
            if (row[14] != DBNull.Value)
            {
                var topAnswer = (string)row[14];
                // TODO: strip HTML?
                AddFacet (facetsNode, FacetType.LongString, "Top Answer", topAnswer);
                // TODO: link to top answer
            }
            #endregion

            var favorites = (int)row[15];
            #region <Facet Name="Is favorite?"><String Value="yes" /></Facet>
            AddFacet (facetsNode, FacetType.String, "Is favorite?", YesNo (favorites > 0));
            #endregion

            #region <Facet Name="Favorites"><Number Value="10" /></Facet>
            AddFacet (facetsNode, FacetType.Number, "Favorites", favorites);
            #endregion

            itemNode.Add (facetsNode);
            #endregion

            return itemNode;
            #endregion
        }

        internal static string YesNo (bool input)
        {
            return input ? "yes" : "no";
        }

        internal static void AddFacet (XElement facets, FacetType facetType, string name, object value)
        {
            AddFacet (facets, facetType, name, new object[] { value });
        }

        internal static void AddFacet (XElement facets, FacetType facetType, string name, IEnumerable<object> values)
        {
            var facetNode = new XElement(CollectionNamespace + "Facet", new XAttribute("Name", name));
            foreach (var value in values)
            {
                var valueNode = new XElement(CollectionNamespace + facetType.ToString(), new XAttribute("Value", value));
                facetNode.Add (valueNode);
            }
            facets.Add (facetNode);
        }

        internal static IEnumerable<object[]> EnumerateRecords 
            (SqlConnection conn, string commandText, IDictionary<string, object> parameters)
        {
            using (var command = conn.CreateCommand ())
            {
                command.CommandText = commandText;
                foreach (var pair in parameters)
                {
                    var param = new SqlParameter(pair.Key, pair.Value);
                    command.Parameters.Add (param);
                }

                using (var reader = command.ExecuteReader (CommandBehavior.SingleResult))
                {
                    while (reader.Read ())
                    {
                        var destination = new object[reader.FieldCount];
                        reader.GetValues (destination);
                        yield return destination;
                    }
                }
            }
        }

        internal static IEnumerable<string> EnumerateTags (SqlConnection conn)
        {
            using (var command = conn.CreateCommand ())
            {
                command.CommandText = SelectTags;
                using (var reader = command.ExecuteReader (CommandBehavior.SingleResult))
                {
                    while (reader.Read ())
                    {
                        yield return reader.GetString(0);
                    }
                }
            }
        }

        internal static string LoadCommandText (string commandName)
        {
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<Program> (commandName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd ();
                return result;
            }
        }
    }
}
