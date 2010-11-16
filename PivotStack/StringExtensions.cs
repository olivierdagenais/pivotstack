using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PivotStack
{
    public static class StringExtensions
    {
        internal static readonly Regex TagsRegex
            = new Regex (@"<(?<tag>[^>]+)>", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        // http://stackoverflow.com/questions/286813/how-do-you-convert-html-to-plain-text/286825#286825
        internal static readonly Regex ElementRegex
            = new Regex (@"<[^>]*>", RegexOptions.Compiled);

        internal static IEnumerable<string> ParseTags (this string tagsColumn)
        {
            var matches = TagsRegex.Matches (tagsColumn);
            foreach (Match match in matches)
            {
                var tag = match.Groups["tag"].Value;
                yield return tag;
            }
        }

        public static string CleanHtml (this string html)
        {
            // TODO: list items (i.e. <li>) should probably have at least a dash inserted on the line, with line breaks
            // TODO: what about links inside the text? we could have Body Links, Accepted Answer Links, Top Answer Links
            // TODO: we should probably convert <strong>bold</strong> to *bold*
            // TODO: StackOverflow will have code samples; how should we filter those?
            var plainText = ElementRegex.Replace (html, String.Empty);
            // TODO: truncate the text (with an elipsis...) to an appropriate length
            return plainText;
        }

        public static string ToBinnedPath (this string fileName, int binSize)
        {
            var withoutExtension = Path.GetFileNameWithoutExtension (fileName);
            var length = withoutExtension.Length;
            var binCount = length / binSize;
            var estimatedCapacity = (binCount - 1) * (binSize + 1) + fileName.Length;
            var sb = new StringBuilder (estimatedCapacity);
            var e = BinUp (withoutExtension, binSize).GetEnumerator ();
            e.MoveNext ();
            while (true)
            {
                var value = e.Current;
                var hasNext = e.MoveNext ();
                if (hasNext)
                {
                    sb.Append (value);
                    sb.Append ('/');
                }
                else
                {
                    break;
                }
            }
            sb.Append (fileName);
            return sb.ToString ();
        }

        public static IEnumerable<string> BinUpReverse (this string input, int binSize)
        {
            int c;
            for (c = input.Length; c >= binSize; c -= binSize)
            {
                var chunk = input.Substring (c - binSize, binSize);
                yield return chunk;
            }
            if (c > 0)
            {
                var chunk = input.Substring (0, c);
                yield return chunk;
            }
        }

        public static IEnumerable<string> BinUp (this string input, int binSize)
        {
            int leftoverCharacters = input.Length % binSize;
            if (leftoverCharacters > 0)
            {
                var chunk = input.Substring (0, leftoverCharacters);
                yield return chunk;
            }
            int c;
            for (c = leftoverCharacters; c < input.Length; c += binSize)
            {
                var chunk = input.Substring (c, binSize);
                yield return chunk;
            }
        }
    }
}
