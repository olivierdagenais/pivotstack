using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        internal static XElement GenerateImageCollection (
            IEnumerable<int> postIds,
            string imageFormat,
            string postFileNameFormat,
            string relativePathToRoot,
            int originalImageWidth,
            int originalImageHeight
            )
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
            collectionNode.SetAttributeValue ("Format", imageFormat);

            // the <Size> element is the same for all <I> elements
            #region <Size Width="800" Height="400" />
            var sizeNode = new XElement (SizeNodeName);
            sizeNode.SetAttributeValue ("Width", originalImageWidth);
            sizeNode.SetAttributeValue ("Height", originalImageHeight);
            #endregion

            var itemsNode = collectionNode.XPathSelectElement ("dz:Items", namespaceManager);

            var mortonNumber = 0;
            var maxPostId = 0;
            foreach (var postId in postIds)
            {
                var itemNode =
                    CreateImageCollectionItemNode (mortonNumber, postId, postFileNameFormat, relativePathToRoot);
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

        internal static XElement CreateImageCollectionItemNode
            (int mortonNumber, int id, string postFileNameFormat, string relativePathToRoot)
        {
            #region <I N="0" Id="351" Source="../../../0/0351.dzi" />
            var itemNode = new XElement (ItemNodeName);
            // "N" is "The number of the item (Morton Number) where it appears in the tiles."
            itemNode.SetAttributeValue ("N", mortonNumber);
            itemNode.SetAttributeValue ("Id", id);
            var relativeDziSubPath = Post.ComputeBinnedPath (id, "dzi", postFileNameFormat);
            var relativeDziPath = Path.Combine (relativePathToRoot, relativeDziSubPath);
            itemNode.SetAttributeValue ("Source", relativeDziPath);
            #endregion

            return itemNode;
        }
    }
}
