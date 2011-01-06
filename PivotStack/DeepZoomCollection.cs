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
    public class DeepZoomCollection
    {
        internal const int CollectionTilePower = 8;
        internal const int CollectionTileSize = 256;

        internal static readonly XNamespace DeepZoom2008Namespace
            = "http://schemas.microsoft.com/deepzoom/2008";

        private static readonly XName ItemNodeName = DeepZoom2008Namespace + "I";
        private static readonly XName SizeNodeName = DeepZoom2008Namespace + "Size";

        private readonly string _postFileNameIdFormat;
        private readonly ImageFormat _imageFormat;
        private readonly string _imageFormatName;
        private readonly int _originalImageWidth;
        private readonly int _originalImageHeight;
        private readonly XmlWriterSettings _writerSettings;

        public DeepZoomCollection (string postFileNameIdFormat, ImageFormat imageFormat, int originalImageWidth,
            int originalImageHeight, XmlWriterSettings writerSettings)
        {
            _postFileNameIdFormat = postFileNameIdFormat;
            _imageFormat = imageFormat;
            // TODO: Add a GetName() extension method to ImageFormat?
            _imageFormatName = null == _imageFormat ? null : _imageFormat.ToString ().ToLower ();
            _originalImageWidth = originalImageWidth;
            _originalImageHeight = originalImageHeight;
            _writerSettings = writerSettings;
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
            namespaceManager.AddNamespace ("dz", DeepZoom2008Namespace.NamespaceName);
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<DeepZoomCollection> ("Template.dzc"))
            using (var reader = new StreamReader(stream))
            {
                doc = XDocument.Parse (reader.ReadToEnd ());
            }
            var collectionNode = doc.Root;
            Debug.Assert (collectionNode != null);
            collectionNode.SetAttributeValue ("Format", _imageFormatName);

            // the <Size> element is the same for all <I> elements
            #region <Size Width="800" Height="400" />
            var sizeNode = new XElement (SizeNodeName);
            sizeNode.SetAttributeValue ("Width", _originalImageWidth);
            sizeNode.SetAttributeValue ("Height", _originalImageHeight);
            #endregion

            var itemsNode = collectionNode.XPathSelectElement ("dz:Items", namespaceManager);

            var mortonNumber = 0;
            var maxPostId = 0;
            foreach (var postId in postIds)
            {
                var itemNode =
                    CreateImageCollectionItemNode (mortonNumber, postId, relativePathToRoot);
                itemNode.Add (sizeNode);
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
            var relativeDziSubPath = Post.ComputeBinnedPath (id, "dzi", _postFileNameIdFormat);
            var relativeDziPath = Path.Combine (relativePathToRoot, relativeDziSubPath);
            itemNode.SetAttributeValue ("Source", relativeDziPath);
            #endregion

            return itemNode;
        }

        public void CreateCollectionManifest(List<int> postIds, string absolutePathToCollectionManifest,
            string relativePathToRoot)
        {
            Directory.CreateDirectory (Path.GetDirectoryName (absolutePathToCollectionManifest));
            var element = GenerateImageCollection (postIds, relativePathToRoot);
            using (var outputStream =
                new FileStream (absolutePathToCollectionManifest, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = XmlWriter.Create (outputStream, _writerSettings))
                {
                    Debug.Assert (writer != null);
                    element.WriteTo (writer);
                }
            }
        }

        internal static IEnumerable<Bitmap> OpenLevelImages
            (IEnumerable<int> postIds, string extension, string fileNameIdFormat, string absoluteOutputPath, int level)
        {
            var levelName = Convert.ToString (level, 10);
            var inputFileName = Path.ChangeExtension (DeepZoomImage.TileZeroZero, extension);
            foreach (var postId in postIds)
            {
                var relativeFolder = Post.ComputeBinnedPath (postId, null, fileNameIdFormat) + "_files";
                var relativeLevelFolder = relativeFolder.CombinePath (levelName, inputFileName);
                var absoluteSourceImagePath = Path.Combine (absoluteOutputPath, relativeLevelFolder);
                using (var bitmap = new Bitmap (absoluteSourceImagePath))
                {
                    yield return bitmap;
                }
            }
        }

        internal static void CreateCollectionTiles(
            Tag tag,
            string outputPath,
            List<int> postIds,
            ImageFormat imageFormat,
            string fileNameIdFormat,
            string absoluteOutputPath
            )
        {
            var extension = imageFormat.ToString ().ToLower ();
            var relativePathToCollectionFolder = Tag.ComputeBinnedPath (tag.Name, null) + "_files";
            var absolutePathToCollectionFolder = Path.Combine (outputPath, relativePathToCollectionFolder);
            for (var level = 0; level < CollectionTilePower; level++)
            {
                var levelName = Convert.ToString (level, 10);
                var absolutePathToCollectionLevelFolder = Path.Combine (absolutePathToCollectionFolder, levelName);
                Directory.CreateDirectory (absolutePathToCollectionLevelFolder);
                var levelSize = (int) Math.Pow (2, level);
                var imageCollectionTiles = GenerateCollectionTiles (postIds, levelSize);
                foreach (var imageCollectionTile in imageCollectionTiles)
                {
                    var relativePathToTile = Path.ChangeExtension (imageCollectionTile.TileName, extension);
                    var absolutePathToTile = Path.Combine (absolutePathToCollectionLevelFolder, relativePathToTile);
                    var levelImages = OpenLevelImages (imageCollectionTile.Ids, extension, fileNameIdFormat,
                                                       absoluteOutputPath, level);
                    using (var bitmap = CreateCollectionTile (levelImages, levelSize))
                    {
                        bitmap.Save (absolutePathToTile, imageFormat);
                    }
                }
            }
        }
    }
}
