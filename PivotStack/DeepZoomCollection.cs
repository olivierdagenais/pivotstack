using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using SoftwareNinjas.Core;

namespace PivotStack
{
    // TODO: some code is for an individual DZC, other is for a DeepZoomCollectionFactory or something similar
    public class DeepZoomCollection
    {
        internal const int CollectionTilePower = 8;
        internal const int CollectionTileSize = 256;

        private static readonly XName ItemNodeName = Namespaces.DeepZoom2008 + "I";
        private static readonly XName SizeNodeName = Namespaces.DeepZoom2008 + "Size";

        private readonly Settings _settings;
        private readonly string _absoluteOutputFolder;

        private readonly string _imageFormatName;
        private readonly XElement _sizeNode;

        public DeepZoomCollection(Settings settings, string absoluteOutputFolder)
        {
            _settings = settings;
            // TODO: Add a GetName() extension method to ImageFormat?
            _imageFormatName =
                null == _settings.PostImageEncoding
                ? null
                : _settings.PostImageEncoding.ToString ().ToLower ();
            _absoluteOutputFolder = absoluteOutputFolder;

            // the <Size> element is the same for all <I> elements
            #region <Size Width="800" Height="400" />
            _sizeNode = new XElement (SizeNodeName);
            _sizeNode.SetAttributeValue ("Width", settings.ItemImageSize.Width);
            _sizeNode.SetAttributeValue ("Height", settings.ItemImageSize.Height);
            #endregion
        }

        internal static Bitmap CreateCollectionTile(IEnumerable<Bitmap> componentBitmaps, int levelSize)
        {
            var result = new Bitmap (CollectionTileSize, CollectionTileSize);
            using (var graphics = Graphics.FromImage (result))
            {
                graphics.InterpolationMode = InterpolationMode.Default;
                var mortonNumber = 0;
                foreach (var itemBitmap in componentBitmaps)
                {
                    var mortonLocation = MortonLayout.Decode (mortonNumber);
                    var destRect = new Rectangle (
                        mortonLocation.X * levelSize,
                        mortonLocation.Y * levelSize,
                        levelSize,
                        levelSize
                    );
                    graphics.DrawImage (
                        itemBitmap,
                        destRect,
                        0,
                        0,
                        levelSize,
                        levelSize,
                        GraphicsUnit.Pixel
                    );
                    mortonNumber++;
                }
            }
            return result;
        }

        internal static IEnumerable<ImageCollectionTile> GenerateCollectionTiles (
            IEnumerable<int> ids,
            int levelSize
        )
        {
            var imagesInEachDimension = NumberOfImagesInEachDimension (levelSize);
            var imagesPerTile = imagesInEachDimension * imagesInEachDimension;
            var mortonNumber = 0;
            var imagesThisTile = 0;

            var currentRow = 0;
            var currentColumn = 0;
            var idsForTile = new List<int> ();
            var startingMortonNumber = 0;
            foreach (var id in ids)
            {
                idsForTile.Add (id);

                mortonNumber++;
                imagesThisTile++;
                if (imagesThisTile == imagesPerTile)
                {
                    var imageCollectionTile = 
                        new ImageCollectionTile (currentRow, currentColumn, startingMortonNumber, idsForTile);
                    yield return imageCollectionTile;
                    startingMortonNumber = mortonNumber;
                    imagesThisTile = 0;
                    var point = MortonLayout.Decode (mortonNumber);
                    currentColumn = point.X / imagesInEachDimension;
                    currentRow = point.Y / imagesInEachDimension;
                    idsForTile.Clear ();
                }
            }
            if (imagesThisTile > 0)
            {
                var imageCollectionTile = 
                    new ImageCollectionTile (currentRow, currentColumn, startingMortonNumber, idsForTile);
                yield return imageCollectionTile;
            }
        }

        internal static int NumberOfImagesInEachDimension (int levelSize)
        {
            return CollectionTileSize / levelSize;
        }

        internal XElement GenerateImageCollection(IEnumerable<int> postIds, string relativePathToRoot)
        {
            XDocument doc;
            var namespaceManager = new XmlNamespaceManager (new NameTable ());
            namespaceManager.AddNamespace ("dz", Namespaces.DeepZoom2008.NamespaceName);
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomCollection> ("Template.dzc"))
            using (var reader = new StreamReader(stream))
            {
                doc = XDocument.Parse (reader.ReadToEnd ());
            }
            var collectionNode = doc.Root;
            Debug.Assert (collectionNode != null);
            collectionNode.SetAttributeValue ("Format", _imageFormatName);

            var itemsNode = collectionNode.XPathSelectElement ("dz:Items", namespaceManager);

            var mortonNumber = 0;
            var maxPostId = 0;
            foreach (var postId in postIds)
            {
                var itemNode =
                    CreateImageCollectionItemNode (mortonNumber, postId, relativePathToRoot);
                itemNode.Add (_sizeNode);
                itemsNode.Add (itemNode);

                mortonNumber++;
                maxPostId = Math.Max (maxPostId, postId);
            }

            // @NextItemId is documented as:
            // "Gets the count of items in the collection; however for Deep Zoom
            // this does not matter because collections are read-only"
            // ...BUT Pivot is very finicky about this one and will consider an
            // entire .dzc invalid if this isn't one more than the highest @Id in the .dzc document.
            collectionNode.SetAttributeValue ("NextItemId", maxPostId + 1);

            return collectionNode;
        }

        internal XElement CreateImageCollectionItemNode(int mortonNumber, int id, string relativePathToRoot)
        {
            #region <I N="0" Id="351" Source="../../../0/0351.dzi" />
            var itemNode = new XElement (ItemNodeName);
            // "N" is "The number of the item (Morton Number) where it appears in the tiles."
            itemNode.SetAttributeValue ("N", mortonNumber);
            itemNode.SetAttributeValue ("Id", id);
            var relativeDziSubPath = Post.ComputeBinnedPath (id, "dzi", _settings.FileNameIdFormat);
            var relativeDziPath = Path.Combine (relativePathToRoot, relativeDziSubPath);
            itemNode.SetAttributeValue ("Source", relativeDziPath);
            #endregion

            return itemNode;
        }

        public void CreateCollectionManifest(Tag tag, List<int> postIds)
        {
            var relativePathToCollectionManifest = tag.ComputeBinnedPath (".dzc");
            var absolutePathToCollectionManifest = Path.Combine (_absoluteOutputFolder, relativePathToCollectionManifest);
            var relativePathToRoot = relativePathToCollectionManifest.RelativizePath ();

            Directory.CreateDirectory (Path.GetDirectoryName (absolutePathToCollectionManifest));
            var element = GenerateImageCollection (postIds, relativePathToRoot);
            using (var outputStream =
                new FileStream (absolutePathToCollectionManifest, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = XmlWriter.Create (outputStream, _settings.XmlWriterSettings))
                {
                    Debug.Assert (writer != null);
                    element.WriteTo (writer);
                }
            }
        }

        internal IEnumerable<Bitmap> OpenLevelImages(IEnumerable<int> postIds, int level)
        {
            var levelName = Convert.ToString (level, 10);
            var inputFileName = Path.ChangeExtension (DeepZoomImage.TileZeroZero, _imageFormatName);
            foreach (var postId in postIds)
            {
                var relativeFolder = Post.ComputeBinnedPath (postId, null, _settings.FileNameIdFormat) + "_files";
                var relativeLevelFolder = relativeFolder.CombinePath (levelName, inputFileName);
                var absoluteSourceImagePath = Path.Combine (_absoluteOutputFolder, relativeLevelFolder);
                using (var bitmap = new Bitmap (absoluteSourceImagePath))
                {
                    yield return bitmap;
                }
            }
        }

        public void CreateCollectionTiles(Tag tag, List<int> postIds)
        {
            var relativePathToCollectionFolder = tag.ComputeBinnedPath (null) + "_files";
            var absolutePathToCollectionFolder = Path.Combine (_absoluteOutputFolder, relativePathToCollectionFolder);
            for (var level = 0; level < CollectionTilePower; level++)
            {
                var levelName = Convert.ToString (level, 10);
                var absolutePathToCollectionLevelFolder = Path.Combine (absolutePathToCollectionFolder, levelName);
                Directory.CreateDirectory (absolutePathToCollectionLevelFolder);
                var levelSize = (int) Math.Pow (2, level);
                var imageCollectionTiles = GenerateCollectionTiles (postIds, levelSize);
                foreach (var imageCollectionTile in imageCollectionTiles)
                {
                    var relativePathToTile = Path.ChangeExtension (imageCollectionTile.TileName, _imageFormatName);
                    var absolutePathToTile = Path.Combine (absolutePathToCollectionLevelFolder, relativePathToTile);
                    var levelImages = OpenLevelImages (imageCollectionTile.Ids, level);
                    using (var bitmap = CreateCollectionTile (levelImages, levelSize))
                    {
                        bitmap.Save (absolutePathToTile, _settings.PostImageEncoding);
                    }
                }
            }
        }

        internal static void PivotizeTag (Tag tag, IEnumerable<StreamReader> streamReaders, Stream destination, string siteDomain, XmlReaderSettings readerSettings, XmlWriterSettings writerSettings)
        {
            XDocument doc;
            XmlNamespaceManager namespaceManager;
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<Program> ("Template.cxml"))
            using (var reader = XmlReader.Create (stream, readerSettings))
            {
                doc = XDocument.Load (reader);
                namespaceManager = new XmlNamespaceManager(reader.NameTable);
                namespaceManager.AddNamespace("c", Namespaces.Collection.NamespaceName);
                namespaceManager.AddNamespace("p", Namespaces.Pivot.NamespaceName);
            }
            var collectionNode = doc.Root;
            Debug.Assert(collectionNode != null);
            collectionNode.SetAttributeValue ("Name", "Tagged Questions: {0}".FormatInvariant (tag.Name));
            // TODO: do we want to strip hyphens from tag for AdditionalSearchText?
            collectionNode.SetAttributeValue (Namespaces.Pivot + "AdditionalSearchText", tag.Name);

            var itemsNode = collectionNode.XPathSelectElement ("c:Items", namespaceManager);
            itemsNode.SetAttributeValue ("HrefBase", "http://{0}/questions/".FormatInvariant (siteDomain));
            itemsNode.SetAttributeValue ("ImgBase", Path.ChangeExtension (tag.Name, ".dzc"));
            using (var writer = new CollectionWriter (destination, writerSettings, futureCw =>
                {
                    futureCw.Flush ();
                    var sw = new StreamWriter (destination);
                    foreach (var sr in streamReaders)
                    {
                        foreach (var line in sr.Lines())
                        {
#if DEBUG
                            sw.WriteLine (line);
#else
                            sw.Write (line);
#endif
                        }
                        sr.Close ();
                    }
                    sw.Flush ();
                })
            )
            {
                doc.Save (writer);
            }
        }
    }
}
