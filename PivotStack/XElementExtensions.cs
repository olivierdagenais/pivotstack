using System.Collections.Generic;
using System.Xml.Linq;

using SoftwareNinjas.Core;

namespace PivotStack
{
    internal static class XElementExtensions
    {
        internal static void AddFacet
            (this XElement facets, FacetType facetType, string name, object value)
        {
            AddFacet (facets, facetType, name, new[] { value });
        }

        internal static void AddFacet
            (this XElement facets, FacetType facetType, string name, IEnumerable<object> values)
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
            (this XElement facets, string facetName, Pair<string, string> hrefNamePair)
        {
            AddFacetLink (facets, facetName, new[] { hrefNamePair });
        }

        internal static void AddFacetLink
            (this XElement facets, string facetName, IEnumerable<Pair<string, string>> hrefNamePairs)
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
