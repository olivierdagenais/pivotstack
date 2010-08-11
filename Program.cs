using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        // http://stackoverflow.com/questions/286813/how-do-you-convert-html-to-plain-text/286825#286825
        internal static readonly Regex ElementRegex
            = new Regex (@"<[^>]*>", RegexOptions.Compiled);

        internal static IEnumerable<string> ParseTags (string tagsColumn)
        {
            var matches = TagsRegex.Matches (tagsColumn);
            foreach (Match match in matches)
            {
                var tag = match.Groups["tag"].Value;
                yield return tag;
            }
        }

        internal static string CleanHtml (string html)
        {
            // TODO: list items (i.e. <li>) should probably have at least a dash inserted on the line, with line breaks
            // TODO: what about links inside the text? we could have Body Links, Accepted Answer Links, Top Answer Links
            // TODO: we should probably convert <strong>bold</strong> to *bold*
            // TODO: StackOverflow will have code samples; how should we filter those?
            var plainText = ElementRegex.Replace (html, String.Empty);
            // TODO: truncate the text (with an elipsis...) to an appropriate length
            return plainText;
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
            Debug.Assert(collectionNode != null);
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
                Debug.Assert(writer != null);
                doc.Save (writer);
            }
        }

        internal static XElement PivotizePost (IList row)
        {
            var post = PostsByTagRow.Load (row);
            #region <Item Id="3232" Href="3232" Name="What are the best Excel tips?">
            var itemNode = new XElement (CollectionNamespace + "Item");

            itemNode.SetAttributeValue ("Id", post.Id);
            itemNode.SetAttributeValue ("Href", post.Id);

            itemNode.SetAttributeValue ("Name", post.Name);

            #region <Description>What are your best tips/not so known features of excel?</Description>
            var descriptionNode = new XElement (CollectionNamespace + "Description", CleanHtml (post.Description));
            itemNode.Add (descriptionNode);
            #endregion

            #region <Facets>
            var facetsNode = new XElement(CollectionNamespace + "Facets");

            #region <Facet Name="Score"><Number Value="7" /></Facet>
            AddFacet (facetsNode, FacetType.Number, "Score", post.Score);
            #endregion

            #region <Facet Name="Views"><Number Value="761" /></Facet>
            AddFacet (facetsNode, FacetType.Number, "Views", post.Views);
            #endregion

            #region <Facet Name="Answers"><Number Value="27" /></Facet>
            AddFacet (facetsNode, FacetType.Number, "Answers", post.Answers);
            #endregion

            if (post.Tags != null)
            {
                var tags = ParseTags (post.Tags);
                #region <Facet Name="Tagged"><String Value="excel" /><String Value="tips-and-tricks" /></Facet>
                AddFacet (facetsNode, FacetType.String, "Tagged", tags.Map (t => (object) t));
                #endregion

                #region <Facet Name="Related Tags"><Link Href="excel.cxml" Name="excel" /></Facet>
                AddFacetLink (facetsNode, "Related Tags", tags.Map (t => new Pair<string, string> (t + ".cxml", t)));
                #endregion
            }

            #region <Facet Name="Date asked"><DateTime Value="2009-07-15T18:41:08" /></Facet>
            AddFacet (facetsNode, FacetType.DateTime, "Date asked", post.DateAsked.ToString ("s"));
            #endregion

            #region <Facet Name="Is answered?"><String Value="yes" /></Facet>
            AddFacet (facetsNode, FacetType.String, "Is answered?", YesNo (post.DateFirstAnswered.HasValue));
            #endregion

            #region <Facet Name="Date first answered"><DateTime Value="2009-07-15T18:41:08" /></Facet>
            if (post.DateFirstAnswered.HasValue)
            {
                AddFacet (facetsNode, FacetType.DateTime, "Date first answered", post.DateFirstAnswered.Value.ToString ("s"));
            }
            #endregion

            #region <Facet Name="Date last answered"><DateTime Value="2010-06-16T09:46:07" /></Facet>
            if (post.DateLastAnswered.HasValue)
            {
                AddFacet (facetsNode, FacetType.DateTime, "Date last answered", post.DateLastAnswered.Value.ToString ("s"));
            }
            #endregion

            #region <Facet Name="Asker"><String Value="Bob" /></Facet>
            if (post.Asker != null)
            {
                AddFacet (facetsNode, FacetType.String, "Asker", post.Asker);
            }
            #endregion

            #region <Facet Name="Has accepted answer?"><String Value="yes" /></Facet>
            AddFacet (facetsNode, FacetType.String, "Has accepted answer?", YesNo (post.AcceptedAnswerId.HasValue));
            #endregion

            #region <Facet Name="Accepted Answer"><LongString Value="My best advice for Excel..." /></Facet>
            if (post.AcceptedAnswer != null)
            {
                AddFacet (facetsNode, FacetType.LongString, "Accepted Answer", CleanHtml (post.AcceptedAnswer));
                // TODO: link to accepted answer
                // Accepted Answer Details: Link, Author(s), Score
            }
            #endregion

            #region <Facet Name="Top Answer"><LongString Value="In-cell graphs..." /></Facet>
            if (post.TopAnswer != null)
            {
                AddFacet (facetsNode, FacetType.LongString, "Top Answer", CleanHtml (post.TopAnswer));
                // TODO: link to top answer
                // Top Answer Details: Link, Author(s), Score
            }
            #endregion

            #region <Facet Name="Is favorite?"><String Value="yes" /></Facet>
            AddFacet (facetsNode, FacetType.String, "Is favorite?", YesNo (post.Favorites > 0));
            #endregion

            #region <Facet Name="Favorites"><Number Value="10" /></Facet>
            AddFacet (facetsNode, FacetType.Number, "Favorites", post.Favorites);
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
            AddFacet (facets, facetType, name, new[] { value });
        }

        internal static void AddFacet (XElement facets, FacetType facetType, string name, IEnumerable<object> values)
        {
            var facetNode = new XElement(CollectionNamespace + "Facet", new XAttribute("Name", name));
            var elementName = CollectionNamespace + facetType.ToString();
            foreach (var value in values)
            {
                var valueNode = new XElement(elementName, new XAttribute("Value", value));
                facetNode.Add (valueNode);
            }
            facets.Add (facetNode);
        }

        internal static void AddFacetLink
            (XElement facets, string facetName, Pair<string, string> hrefNamePair)
        {
            AddFacetLink (facets, facetName, new[] { hrefNamePair });
        }

        internal static void AddFacetLink
            (XElement facets, string facetName, IEnumerable<Pair<string, string>> hrefNamePairs)
        {
            var facetNode = new XElement (CollectionNamespace + "Facet", new XAttribute ("Name", facetName));
            var elementName = CollectionNamespace + FacetType.Link.ToString ();
            foreach (var pair in hrefNamePairs)
            {
                var href = pair.First;
                var name = pair.Second;
                var linkNode = new XElement (elementName, new XAttribute ("Href", href), new XAttribute ("Name", name));
                facetNode.Add (linkNode);
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
                    Debug.Assert(reader != null);
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
                    Debug.Assert(reader != null);
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
