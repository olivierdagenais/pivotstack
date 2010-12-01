using System.Collections.Generic;
using System.Data;
using SoftwareNinjas.Core;

namespace PivotStack.Repositories
{
    // TODO: eventually extract ITagRepository and move this implementation to Repositories.Database
    public class TagRepository : DatabaseRepositoryBase
    {
        internal static readonly string SelectTags = LoadCommandText ("select-tags.sql");

        public TagRepository (IDbConnection connection) : base(connection)
        {
        }

        public IEnumerable<Tag> RetrieveTags()
        {
            var rows = EnumerateRecords (SelectTags);
            var tags = rows.Map (row => Tag.LoadFromRow (row));
            return tags;
        }
    }
}
