using System.Data;
using System.Collections.Generic;

using SoftwareNinjas.Core;

namespace PivotStack.Repositories
{
    // TODO: eventually extract IPostRepository and move this implementation to Repositories.Database
    public class PostRepository : DatabaseRepositoryBase
    {
        internal static readonly string SelectPosts = LoadCommandText ("select-posts.sql");
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

        public IEnumerable<int> RetrievePostIds (int tagId)
        {
            var parameters = new Dictionary<string, object> { { "@tagId", tagId } };
            var rows = EnumerateRecords (SelectPostsByTag, parameters);
            var postIds = rows.Map (row => (int) row[0]);
            return postIds;
        }
    }
}
