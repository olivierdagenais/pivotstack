using System.Collections;

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
    }
}
