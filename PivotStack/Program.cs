﻿using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows.Controls;
using PivotStack.Repositories;

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
        private const string WorkingFolderName = "rawItems";

        internal static readonly XNamespace CollectionNamespace
            = "http://schemas.microsoft.com/collection/metadata/2009";
        internal static readonly XNamespace PivotNamespace 
            = "http://schemas.microsoft.com/livelabs/pivot/collection/2009";
        internal static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
        {
            NewLineChars = "\n",
#if DEBUG
            Indent = true,
            IndentChars = "  ",
#endif
        };
        internal const SaveOptions PostSaveOptions =
#if DEBUG
            SaveOptions.None
#else
            SaveOptions.DisableFormatting
#endif
        ;

        [STAThread]
        public static int Main (string[] args)
        {
            // TODO: initialize Settings instance from app.config and/or command-line
            var settings = new Settings
            {
                /*
                DatabaseConnectionString = "Data Source=Blackberry;Initial Catalog=SuperUser;Integrated Security=True",
                SiteDomain = "superuser.com",
                MaximumNumberOfItems = 185520,
                HighestId = 207698,
                 */
                DatabaseConnectionString = "Data Source=Blackberry;Initial Catalog=Photography;Integrated Security=True",
                SiteDomain = "photo.stackexchange.com",
                MaximumNumberOfItems = 936,
                HighestId = 4479,

                PostImageEncoding = ImageFormat.Png,
            };

            using (var tagsConnection = new SqlConnection(settings.DatabaseConnectionString))
            using (var postsConnection = new SqlConnection (settings.DatabaseConnectionString))
            {
                tagsConnection.Open ();
                postsConnection.Open ();
                var tagRepository = new TagRepository (tagsConnection);
                var postRepository = new PostRepository (postsConnection);

                // Phase 1: Convert Posts (collection items) into temporary raw artifacts
                //CreateRawItems (settings, postRepository);

                #region Phase 2: Slice Post (collection item) images to create final .dzi files and sub-folders
                // TODO: foreach post in posts slice corresponding image into tiles for all zoom levels
                #endregion

                #region Phase 3: Convert Tags (collections) into final .cxml and .dzc files
                var tags = tagRepository.RetrieveTags ();
                foreach (var tag in tags)
                {
                    // TODO: Generate .cxml file for tag
                    // TODO: Generate .dzc file for tag
                }
                #endregion
            }
            return 0;
        }

        internal static void CreateRawItems (Settings settings, PostRepository postRepository)
        {
            var workingPath = Path.GetFullPath (WorkingFolderName);
            if (Directory.Exists (workingPath))
            {
                Directory.Delete (workingPath, true);
            }
            Directory.CreateDirectory (workingPath);
            Page template;
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<Program> ("Template.xaml"))
            {
                template = (Page) XamlReader.Load (stream);
            }
            var imageFormat = settings.PostImageEncoding;
            var imageExtension = imageFormat.ToString ().ToLower ();

            var posts = postRepository.RetrievePosts ();
            foreach (var post in posts)
            {
                var relativeBinnedXmlPath = post.ComputeBinnedPath (".xml", settings.FileNameIdFormat);
                var absoluteBinnedXmlPath = Path.Combine (workingPath, relativeBinnedXmlPath);
                Directory.CreateDirectory (Path.GetDirectoryName (absoluteBinnedXmlPath));
                var element = PivotizePost (post);
                using (var sw = new StreamWriter (absoluteBinnedXmlPath, false, Encoding.UTF8))
                {
                    var str = element.ToString (PostSaveOptions);
                    sw.Write (str);
                }

                var relativeBinnedImagePath = post.ComputeBinnedPath (imageExtension, settings.FileNameIdFormat);
                var absoluteBinnedImagePath = Path.Combine (workingPath, relativeBinnedImagePath);
                Directory.CreateDirectory (Path.GetDirectoryName (absoluteBinnedImagePath));
                using (var outputStream
                    = new FileStream (absoluteBinnedImagePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    ImagePost (post, template, imageFormat, outputStream);
                }
            }
        }

        internal static void PivotizeTag (PostRepository postRepository, string tag, string siteDomain)
        {
            using (var outputStream 
                = new FileStream (tag + ".cxml", FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                // TODO: instead of re-processing posts, only load post IDs for the tag and then concatenate their XML
                var posts = postRepository.RetrievePosts (tag);
                PivotizeTag (tag, posts, outputStream, siteDomain);
            }
        }

        internal static void ImagePost (Post post, Page pageTemplate, ImageFormat imageFormat, Stream destination)
        {
            pageTemplate.DataBindAndWait (post);

            var imageSource = pageTemplate.ToBitmapSource ();
            var bitmap = imageSource.ConvertToGdiPlusBitmap ();
            bitmap.Save (destination, imageFormat);
        }

        internal static void PivotizeTag (string tag, IEnumerable<Post> posts, Stream destination, string siteDomain)
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
            itemsNode.SetAttributeValue ("HrefBase", "http://{0}/questions/".FormatInvariant (siteDomain));
            foreach (var post in posts)
            {
                var element = PivotizePost (post);
                itemsNode.Add (element);
            }
            using (var writer = XmlWriter.Create (destination, WriterSettings))
            {
                Debug.Assert(writer != null);
                doc.Save (writer);
            }
        }

        internal static XElement PivotizePost (Post post)
        {
            #region <Item Id="3232" Href="3232" Name="What are the best Excel tips?">
            var itemNode = new XElement ("Item");

            itemNode.SetAttributeValue ("Id", post.Id);
            itemNode.SetAttributeValue ("Href", post.Id);

            itemNode.SetAttributeValue ("Name", post.Name);

            #region <Description>What are your best tips/not so known features of excel?</Description>
            var descriptionNode = new XElement ("Description", post.Description.CleanHtml ());
            itemNode.Add (descriptionNode);
            #endregion

            #region <Facets>
            var facetsNode = new XElement("Facets");

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
                var tags = post.Tags.ParseTags ();
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
            AddFacet (facetsNode, FacetType.String, "Is answered?", post.DateFirstAnswered.HasValue.YesNo());
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
            AddFacet (facetsNode, FacetType.String, "Has accepted answer?", post.AcceptedAnswerId.HasValue.YesNo());
            #endregion

            #region <Facet Name="Accepted Answer"><LongString Value="My best advice for Excel..." /></Facet>
            if (post.AcceptedAnswer != null)
            {
                AddFacet (facetsNode, FacetType.LongString, "Accepted Answer", post.AcceptedAnswer.CleanHtml ());
                // TODO: link to accepted answer
                // Accepted Answer Details: Link, Author(s), Score
            }
            #endregion

            #region <Facet Name="Top Answer"><LongString Value="In-cell graphs..." /></Facet>
            if (post.TopAnswer != null)
            {
                AddFacet (facetsNode, FacetType.LongString, "Top Answer", post.TopAnswer.CleanHtml ());
                // TODO: link to top answer
                // Top Answer Details: Link, Author(s), Score
            }
            #endregion

            #region <Facet Name="Is favorite?"><String Value="yes" /></Facet>
            AddFacet (facetsNode, FacetType.String, "Is favorite?", ( post.Favorites > 0 ).YesNo());
            #endregion

            #region <Facet Name="Favorites"><Number Value="10" /></Facet>
            AddFacet (facetsNode, FacetType.Number, "Favorites", post.Favorites);
            #endregion

            itemNode.Add (facetsNode);
            #endregion

            return itemNode;
            #endregion
        }

        // TODO: turn these 4 methods into extension methods on XElement or instance methods on something else (Item?)
        internal static void AddFacet (XElement facets, FacetType facetType, string name, object value)
        {
            AddFacet (facets, facetType, name, new[] { value });
        }

        internal static void AddFacet (XElement facets, FacetType facetType, string name, IEnumerable<object> values)
        {
            var facetNode = new XElement("Facet", new XAttribute("Name", name));
            var elementName = facetType.ToString();
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
            var facetNode = new XElement ("Facet", new XAttribute ("Name", facetName));
            var elementName = FacetType.Link.ToString ();
            foreach (var pair in hrefNamePairs)
            {
                var href = pair.First;
                var name = pair.Second;
                var linkNode = new XElement (elementName, new XAttribute ("Href", href), new XAttribute ("Name", name));
                facetNode.Add (linkNode);
            }
            facets.Add (facetNode);
        }
    }
}
