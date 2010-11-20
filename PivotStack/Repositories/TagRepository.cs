using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace PivotStack.Repositories
{
    // TODO: eventually extract ITagRepository and move this implementation to Repositories.Database
    public class TagRepository : DatabaseRepositoryBase
    {
        internal static readonly string SelectTags = LoadCommandText ("select-tags.sql");

        public TagRepository (IDbConnection connection) : base(connection)
        {
        }

        public IEnumerable<string> RetrieveTags()
        {
            using (var command = Connection.CreateCommand ())
            {
                command.CommandText = SelectTags;
                using (var reader = command.ExecuteReader (CommandBehavior.SingleResult))
                {
                    Debug.Assert (reader != null);
                    while (reader.Read ())
                    {
                        yield return reader.GetString (0);
                    }
                }
            }
        }
    }
}
