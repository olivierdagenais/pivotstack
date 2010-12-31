using System.Collections;
using System.IO;

namespace PivotStack
{
    public struct Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static Tag LoadFromRow (IList row)
        {
            var result = new Tag
            {
                Id = (int)row[0],
                Name = (string)row[1],
            };
            return result;
        }

        public string ComputeBinnedPath (string extension)
        {
            return ComputeBinnedPath (Name, extension);
        }

        internal static string ComputeBinnedPath (string name, string extension)
        {
            var fileName = Path.ChangeExtension (name, extension);
            var binnedPath = fileName.ToBinnedPath (3);
            return binnedPath;
        }
    }
}
