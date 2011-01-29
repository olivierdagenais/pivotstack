using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using SoftwareNinjas.Core;

namespace PivotStack
{
    public class Settings
    {
        public readonly string SiteDomain;
        public readonly int MaximumNumberOfItems;
        public readonly int HighestId;
        public readonly ImageFormat PostImageEncoding;
        // TODO: path to XAML template?
        public readonly string PathToFavIcon;
        public readonly string PathToBrandImage;
        public readonly string DatabaseConnectionString;
        public readonly string AbsoluteWorkingFolder;
        public readonly string AbsoluteOutputFolder;
        public readonly Size ItemImageSize;
        // TODO: Do we really need to be able to parameterize TileSize & TileOverlap?
        public readonly int TileSize;
        public readonly int TileOverlap;
        // TODO: DeepZoom collection parameters?
        // TODO: Number of threads to use?
        public readonly XmlReaderSettings XmlReaderSettings;
        public readonly XmlWriterSettings XmlWriterSettings;

        public readonly int MaximumNumberOfDigits;

        public readonly string FileNameIdFormat;

        public Settings(SettingsBuilder builder)
        {
            SiteDomain = builder.SiteDomain;
            MaximumNumberOfItems = builder.MaximumNumberOfItems;
            HighestId = builder.HighestId;
            PostImageEncoding = builder.PostImageEncoding;
            PathToFavIcon = builder.PathToFavIcon;
            PathToBrandImage = builder.PathToBrandImage;
            DatabaseConnectionString = builder.DatabaseConnectionString;
            AbsoluteWorkingFolder = builder.AbsoluteWorkingFolder;
            AbsoluteOutputFolder = builder.AbsoluteOutputFolder;
            ItemImageSize = builder.ItemImageSize;
            TileSize = builder.TileSize;
            TileOverlap = builder.TileOverlap;
            XmlReaderSettings = builder.XmlReaderSettings;
            XmlWriterSettings = builder.XmlWriterSettings;

            MaximumNumberOfDigits = 1 + (int) Math.Log10 (Math.Max(1, HighestId));
            FileNameIdFormat = new String ('0', MaximumNumberOfDigits);
        }

        internal XElement GenerateImageManifest()
        {
            XDocument doc;
            XmlNamespaceManager namespaceManager;
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<Settings> ("Template.dzi"))
            using (var reader = XmlReader.Create (stream, XmlReaderSettings))
            {
                doc = XDocument.Load (reader);
                namespaceManager = new XmlNamespaceManager(reader.NameTable);
                namespaceManager.AddNamespace("dz", Namespaces.DeepZoom2009.NamespaceName);
            }
            var imageNode = doc.Root;
            Debug.Assert (imageNode != null);
            #region <Image TileSize="254" Overlap="1" Format="png">
            imageNode.SetAttributeValue ("TileSize", TileSize);
            imageNode.SetAttributeValue ("Overlap", TileOverlap);
            imageNode.SetAttributeValue ("Format", PostImageEncoding.GetName ());

            #region <Size Width="800" Height="400" />
            var sizeNode = imageNode.XPathSelectElement ("dz:Size", namespaceManager);
            sizeNode.SetAttributeValue ("Width", ItemImageSize.Width);
            sizeNode.SetAttributeValue ("Height", ItemImageSize.Height);
            #endregion
            #endregion

            return imageNode;
        }
    }
}
