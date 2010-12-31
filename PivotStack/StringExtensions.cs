using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SoftwareNinjas.Core;

namespace PivotStack
{
    public static class StringExtensions
    {
        internal static readonly Regex TagsRegex
            = new Regex (@"<(?<tag>[^>]+)>", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        // http://stackoverflow.com/questions/286813/how-do-you-convert-html-to-plain-text/286825#286825
        internal static readonly Regex ElementRegex
            = new Regex (@"<[^>]*>", RegexOptions.Compiled);

        private static readonly string[] ReservedDeviceNames = new[]
        {
            "CON",
            "PRN",
            "AUX",
            "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        };

        private static readonly IList<Pair<string, string>> ReservedCharacters =
            new List<Pair<string, string>> (GenerateReservedCharacters ());

        internal static IEnumerable<Pair<string, string>> GenerateReservedCharacters()
        {
            // including % because we use % to escape
            var characters = '%'.Compose (Path.GetInvalidPathChars ());
            var result = characters.Map (c =>
                {
                    var i = Convert.ToInt32 (c);
                    var pair = new Pair<string, string> (Convert.ToString (c), "%{0:x}".FormatInvariant (i));
                    return pair;
                }
            );
            return result;
        }

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

        internal static string RelativizePath(this string relativePath)
        {
            var result = new StringBuilder ();
            foreach (var c in relativePath)
            {
                if (Path.DirectorySeparatorChar == c || Path.AltDirectorySeparatorChar == c)
                {
                    result.Append ("../");
                }
            }
            return result.ToString ();
        }

        /// <remarks>
        /// This version does not strip the folder names nor check for invalid characters, like
        /// <see cref="Path.GetFileNameWithoutExtension(string)"/> would do.
        /// </remarks>
        internal static string GetFileNameWithoutExtension(string fileName)
        {
            var lastDot = fileName.LastIndexOf ('.');
            var result = -1 == lastDot ? fileName : fileName.Substring (0, lastDot);
            return result;
        }

        public static string ToBinnedPath (this string fileName, int binSize)
        {
            var withoutExtension = GetFileNameWithoutExtension (fileName);
            var length = withoutExtension.Length;
            if (length < binSize)
            {
                return fileName;
            }
            var binCount = length / binSize;
            var estimatedCapacity = (binCount - 1) * (binSize + 1) + fileName.Length;
            var sb = new StringBuilder (estimatedCapacity);
            var e = BinUp (withoutExtension, binSize).GetEnumerator ();
            e.MoveNext ();
            while (true)
            {
                var value = SanitizeName(e.Current);
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
            sb.Append (SanitizeName(fileName));
            return sb.ToString ();
        }

        private const char SubstitutionCharacter = '_';
        /// <remarks>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/aa365247%28v=vs.85%29.aspx#naming_conventions">
        /// Naming Files, Paths, and Namespaces > Naming Conventions
        /// </seealso>
        /// </remarks>
        internal static string SanitizeName(string name)
        {
            var result = new StringBuilder (name);

            var matchedDeviceName = false;
            if (3 == name.Length || 4 == name.Length)
            {
                foreach (var reservedDeviceName in ReservedDeviceNames)
                {
                    if (name.Equals (reservedDeviceName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // TODO: this means "con" and "_con" would both map to "_con", causing confusion if not conflict
                        result.Insert (0, SubstitutionCharacter);
                        matchedDeviceName = true;
                        break;
                    }
                }
            }

            if (!matchedDeviceName)
            {
                foreach (var reservedCharacter in ReservedCharacters)
                {
                    result.Replace (reservedCharacter.First, reservedCharacter.Second);
                }
            }

            return result.ToString ();
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
