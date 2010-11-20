using System.Data;
using System.Collections.Generic;

using SoftwareNinjas.Core;

namespace PivotStack.Repositories
{
    public class PostRepository : DatabaseRepositoryBase
    {
        internal static readonly string SelectPosts = LoadCommandText ("select-posts.sql");
        // TODO: selecting by tag is 99% like selecting posts; can we avoid the duplication in the SQL code?
        internal static readonly string SelectPostsByTag = LoadCommandText ("select-posts-by-tag.sql");

        public PostRepository (IDbConnection connection) : base (connection)
        {
        }

        public IEnumerable<Post> RetrievePosts()
        {
            var rows = EnumerateRecords (SelectPosts);
            var posts = rows.Map (row => Post.LoadFromRow (row));
            return posts;
        }

        public IEnumerable<Post> RetrievePosts(string tag)
        {
            var parameters = new Dictionary<string, object> { { "@tag", tag } };
            var rows = EnumerateRecords (SelectPostsByTag, parameters);
            var posts = rows.Map (row => Post.LoadFromRow (row));
            return posts;
        }
    }
}
