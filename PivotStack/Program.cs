using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private const string OutputFolderName = "output";

        private readonly Settings _settings;

        [STAThread]
        public static int Main (string[] args)
        {
            // TODO: initialize Settings instance from app.config and/or command-line
            var settings = new Settings
            {
                ItemImageSize = new Size(800, 400),
                TileSize = 254,
                TileOverlap = 1,
                XmlReaderSettings = new XmlReaderSettings
                {
#if DEBUG
                    IgnoreWhitespace = false,
#else
                    IgnoreWhitespace = true,
#endif
                },
                XmlWriterSettings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    NewLineChars = "\n",
#if DEBUG
                    Indent = true,
                    IndentChars = "  ",
#else
                    NewLineHandling = NewLineHandling.Entitize,
#endif
                },
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

                AbsoluteWorkingFolder = Path.GetFullPath (WorkingFolderName),
                AbsoluteOutputFolder = Path.GetFullPath (OutputFolderName),
                PostImageEncoding = ImageFormat.Png,
            };

            var program = new Program (settings);
            program.Generate ();

            return 0;
        }

        internal void Generate ()
        {
            using (var tagsConnection = new SqlConnection(_settings.DatabaseConnectionString))
            using (var postsConnection = new SqlConnection (_settings.DatabaseConnectionString))
            {
                tagsConnection.Open ();
                postsConnection.Open ();
                var tagRepository = new TagRepository (tagsConnection);
                var postRepository = new PostRepository (postsConnection);

                #region Phase 1: Convert Posts (collection items) into temporary raw artifacts
                CleanWorkingFolder ();
                CreateRawItems (postRepository);
                GeneratePostImageResizes (postRepository);
                #endregion

                #region Phase 2: Slice Post (collection item) images to create final .dzi files and sub-folders
                GenerateImageSlices (postRepository);
                GenerateImageManifests (postRepository);
                #endregion

                #region Phase 3: Convert Tags (collections) into final .cxml and .dzc files
                AssembleCollections (tagRepository, postRepository);
                #endregion
            }
        }

        public Program(Settings settings)
        {
            _settings = settings;
        }

        internal void GenerateImageManifests (PostRepository postRepository)
        {
            var fileNameIdFormat = _settings.FileNameIdFormat;
            var imageNode = Settings.GenerateImageManifest (_settings.TileSize, _settings.TileOverlap,
                                                   _settings.PostImageEncoding.GetName (),
                                                   _settings.ItemImageSize.Width, _settings.ItemImageSize.Height,
                                                   _settings.XmlReaderSettings);

            var sb = new StringBuilder ();
            using (var writer = XmlWriter.Create (sb, _settings.XmlWriterSettings))
            {
                Debug.Assert (writer != null);
                imageNode.WriteTo (writer);
            }
            var imageManifest = sb.ToString ();

            foreach (var postId in postRepository.RetrievePostIds ())
            {
                var relativeBinnedImageManifestPath = Post.ComputeBinnedPath (postId, "dzi", fileNameIdFormat);
                var absoluteBinnedImageManifestPath =
                    Path.Combine (_settings.AbsoluteOutputFolder, relativeBinnedImageManifestPath);
                Directory.CreateDirectory (Path.GetDirectoryName (absoluteBinnedImageManifestPath));
                File.WriteAllText (absoluteBinnedImageManifestPath, imageManifest, Encoding.UTF8);
            }
        }

        internal void GenerateImageSlices(PostRepository postRepository)
        {
            var dzi = new DeepZoomImage (_settings);
            foreach (var postId in postRepository.RetrievePostIds ())
            {
                dzi.SlicePostImage (postId);
            }
        }

        internal void AssembleCollections (TagRepository tagRepository, PostRepository postRepository)
        {
            var dzc = new DeepZoomCollection (_settings);

            var tags = tagRepository.RetrieveTags ();
            foreach (var tag in tags)
            {
                var postIds = new List<int> (postRepository.RetrievePostIds (tag.Id));
                dzc.PivotizeTag (tag, postIds);
                dzc.CreateCollectionManifest (tag, postIds);
                dzc.CreateCollectionTiles (tag, postIds);
            }
        }

        internal void CleanWorkingFolder()
        {
            if (Directory.Exists (_settings.AbsoluteWorkingFolder))
            {
                Directory.Delete (_settings.AbsoluteWorkingFolder, true);
            }
        }

        internal void CreateRawItems (PostRepository postRepository)
        {
            var workingPath = _settings.AbsoluteWorkingFolder;
            Directory.CreateDirectory (workingPath);
            Page template;
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<Program> ("Template.xaml"))
            {
                template = (Page) XamlReader.Load (stream);
                template.Width = _settings.ItemImageSize.Width;
                template.Height = _settings.ItemImageSize.Height;
            }
            var imageFormat = _settings.PostImageEncoding;
            var imageExtension = imageFormat.GetName ();

            var posts = postRepository.RetrievePosts ();
            foreach (var post in posts)
            {
                var relativeBinnedXmlPath = post.ComputeBinnedPath (".xml", _settings.FileNameIdFormat);
                var absoluteBinnedXmlPath = Path.Combine (workingPath, relativeBinnedXmlPath);
                Directory.CreateDirectory (Path.GetDirectoryName (absoluteBinnedXmlPath));
                var element = PivotizePost (post);
                using (var outputStream =
                    new FileStream (absoluteBinnedXmlPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (var writer = new ItemWriter (outputStream, _settings.XmlWriterSettings))
                    {
                        element.Save (writer);
                    }
                }

                var relativeBinnedImagePath = post.ComputeBinnedPath (imageExtension, _settings.FileNameIdFormat);
                var absoluteBinnedImagePath = Path.Combine (workingPath, relativeBinnedImagePath);
                Directory.CreateDirectory (Path.GetDirectoryName (absoluteBinnedImagePath));
                using (var outputStream
                    = new FileStream (absoluteBinnedImagePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    ImagePost (post, template, imageFormat, outputStream);
                }
            }
        }

        internal void GeneratePostImageResizes (PostRepository postRepository)
        {
            var dzi = new DeepZoomImage (_settings);

            var workingPath = _settings.AbsoluteWorkingFolder;
            var imageFormat = _settings.PostImageEncoding;
            var extension = imageFormat.GetName ();
            var fileNameIdFormat = _settings.FileNameIdFormat;
            foreach (var postId in postRepository.RetrievePostIds ())
            {
                var relativeBinnedImagePath = Post.ComputeBinnedPath (postId, extension, fileNameIdFormat);
                var absoluteBinnedImagePath = Path.Combine (workingPath, relativeBinnedImagePath);
                var relativeBinnedImageFolder = Post.ComputeBinnedPath (postId, null, fileNameIdFormat) + "_files";
                var absoluteBinnedImageFolder = Path.Combine (workingPath, relativeBinnedImageFolder);
                Directory.CreateDirectory (absoluteBinnedImageFolder);
                using (var inputStream =
                    new FileStream (absoluteBinnedImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var sourceBitmap = new Bitmap (inputStream))
                {
                    dzi.GeneratePostImageResizes (sourceBitmap, (level, resizedBitmap) =>
                        {
                            var levelImageName = "{0}.{1}".FormatInvariant (level, extension);
                            var levelImagePath = Path.Combine (absoluteBinnedImageFolder, levelImageName);
                            using (var outputStream =
                                new FileStream (levelImagePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                resizedBitmap.Save (outputStream, imageFormat);
                            }
                        }
                    );
                }
            }
        }

        internal static void ImagePost (Post post, Page pageTemplate, ImageFormat imageFormat, Stream destination)
        {
            pageTemplate.DataBindAndWait (post);

            var imageSource = pageTemplate.ToBitmapSource ();
            var bitmap = imageSource.ConvertToGdiPlusBitmap ();
            bitmap.Save (destination, imageFormat);
        }

        internal static XElement PivotizePost (Post post)
        {
            #region <Item Id="3232" Href="3232" Name="What are the best Excel tips?">
            var itemNode = new XElement ("Item");

            itemNode.SetAttributeValue ("Img", "#{0}".FormatInvariant(post.Id));
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
            facetsNode.AddFacet (FacetType.Number, "Score", post.Score);
            #endregion

            #region <Facet Name="Views"><Number Value="761" /></Facet>
            facetsNode.AddFacet (FacetType.Number, "Views", post.Views);
            #endregion

            #region <Facet Name="Answers"><Number Value="27" /></Facet>
            facetsNode.AddFacet (FacetType.Number, "Answers", post.Answers);
            #endregion

            if (post.Tags != null)
            {
                var tags = post.Tags.ParseTags ();
                #region <Facet Name="Tagged"><String Value="excel" /><String Value="tips-and-tricks" /></Facet>
                facetsNode.AddFacet (FacetType.String, "Tagged", tags.Map (t => (object) t));
                #endregion

                #region <Facet Name="Related Tags"><Link Href="excel.cxml" Name="excel" /></Facet>
                // TODO: the "related tags" files should be binned!
                facetsNode.AddFacetLink ("Related Tags", tags.Map (t => new Pair<string, string> (t + ".cxml", t)));
                #endregion
            }

            #region <Facet Name="Date asked"><DateTime Value="2009-07-15T18:41:08" /></Facet>
            facetsNode.AddFacet (FacetType.DateTime, "Date asked", post.DateAsked.ToString ("s"));
            #endregion

            #region <Facet Name="Is answered?"><String Value="yes" /></Facet>
            facetsNode.AddFacet (FacetType.String, "Is answered?", post.DateFirstAnswered.HasValue.YesNo ());
            #endregion

            #region <Facet Name="Date first answered"><DateTime Value="2009-07-15T18:41:08" /></Facet>
            if (post.DateFirstAnswered.HasValue)
            {
                facetsNode.AddFacet (FacetType.DateTime, "Date first answered", post.DateFirstAnswered.Value.ToString ("s"));
            }
            #endregion

            #region <Facet Name="Date last answered"><DateTime Value="2010-06-16T09:46:07" /></Facet>
            if (post.DateLastAnswered.HasValue)
            {
                facetsNode.AddFacet (FacetType.DateTime, "Date last answered", post.DateLastAnswered.Value.ToString ("s"));
            }
            #endregion

            #region <Facet Name="Asker"><String Value="Bob" /></Facet>
            if (post.Asker != null)
            {
                facetsNode.AddFacet (FacetType.String, "Asker", post.Asker);
            }
            #endregion

            #region <Facet Name="Has accepted answer?"><String Value="yes" /></Facet>
            facetsNode.AddFacet (FacetType.String, "Has accepted answer?", post.AcceptedAnswerId.HasValue.YesNo ());
            #endregion

            #region <Facet Name="Accepted Answer"><LongString Value="My best advice for Excel..." /></Facet>
            if (post.AcceptedAnswer != null)
            {
                facetsNode.AddFacet (FacetType.LongString, "Accepted Answer", post.AcceptedAnswer.CleanHtml ());
                // TODO: link to accepted answer
                // Accepted Answer Details: Link, Author(s), Score
            }
            #endregion

            #region <Facet Name="Top Answer"><LongString Value="In-cell graphs..." /></Facet>
            if (post.TopAnswer != null)
            {
                facetsNode.AddFacet (FacetType.LongString, "Top Answer", post.TopAnswer.CleanHtml ());
                // TODO: link to top answer
                // Top Answer Details: Link, Author(s), Score
            }
            #endregion

            #region <Facet Name="Is favorite?"><String Value="yes" /></Facet>
            facetsNode.AddFacet (FacetType.String, "Is favorite?", ( post.Favorites > 0 ).YesNo ());
            #endregion

            #region <Facet Name="Favorites"><Number Value="10" /></Facet>
            facetsNode.AddFacet (FacetType.Number, "Favorites", post.Favorites);
            #endregion

            itemNode.Add (facetsNode);
            #endregion

            return itemNode;
            #endregion
        }
    }
}
