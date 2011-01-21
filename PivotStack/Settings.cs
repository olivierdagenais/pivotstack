﻿using System;
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
        public string SiteDomain { get; set; }
        public int MaximumNumberOfItems { get; set; }
        public int HighestId { get; set; }
        public ImageFormat PostImageEncoding { get; set; }
        // TODO: path to XAML template?
        public string PathToFavIcon { get; set; }
        public string PathToBrandImage { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string AbsoluteWorkingFolder { get; set; }
        public string AbsoluteOutputFolder { get; set; }
        public Size ItemImageSize { get; set; }
        // TODO: Do we really need to be able to parameterize TileSize & TileOverlap?
        public int TileSize { get; set; }
        public int TileOverlap { get; set; }
        // TODO: DeepZoom collection parameters?
        // TODO: Number of threads to use?
        public XmlReaderSettings XmlReaderSettings { get; set; }
        public XmlWriterSettings XmlWriterSettings { get; set; }

        public int MaximumNumberOfDigits
        {
            get
            {
                return 1 + (int) Math.Log10 (HighestId);
            }
        }

        public string FileNameIdFormat
        {
            get
            {
                return new String ('0', MaximumNumberOfDigits);
            }
        }

        internal static XElement GenerateImageManifest
            (int tileSize, int tileOverlap, string imageFormat, int imageWidth, int imageHeight,
             XmlReaderSettings xmlReaderSettings)
        {
            XDocument doc;
            XmlNamespaceManager namespaceManager;
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<Program> ("Template.dzi"))
            using (var reader = XmlReader.Create (stream, xmlReaderSettings))
            {
                doc = XDocument.Load (reader);
                namespaceManager = new XmlNamespaceManager(reader.NameTable);
                namespaceManager.AddNamespace("dz", Namespaces.DeepZoom2009.NamespaceName);
            }
            var imageNode = doc.Root;
            Debug.Assert (imageNode != null);
            #region <Image TileSize="254" Overlap="1" Format="png">
            imageNode.SetAttributeValue ("TileSize", tileSize);
            imageNode.SetAttributeValue ("Overlap", tileOverlap);
            imageNode.SetAttributeValue ("Format", imageFormat);

            #region <Size Width="800" Height="400" />
            var sizeNode = imageNode.XPathSelectElement ("dz:Size", namespaceManager);
            sizeNode.SetAttributeValue ("Width", imageWidth);
            sizeNode.SetAttributeValue ("Height", imageHeight);
            #endregion
            #endregion

            return imageNode;
        }
    }
}
