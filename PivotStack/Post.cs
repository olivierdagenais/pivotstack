using System;
using System.Collections;

namespace PivotStack
{
    public struct Post
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
        public int Views { get; set; }
        public int Answers { get; set; }
        public string Tags { get; set; }
        public DateTime DateAsked { get; set; }
        public DateTime? DateFirstAnswered { get; set; }
        public DateTime? DateLastAnswered { get; set; }
        public string Asker { get; set; }
        public int? AcceptedAnswerId { get; set; }
        public string AcceptedAnswer { get; set; }
        public int? TopAnswerId { get; set; }
        public string TopAnswer { get; set; }
        public int Favorites { get; set; }

        public static Post LoadFromRow (IList row)
        {
            var result = new Post
            {
                Id = (int)row[0],
                Name = (string)row[1],
                Description = (string)row[2],
                Score = (int)row[3],
                Views = (int)row[4],
                Answers = (int)row[5],
                Tags = row[6].FromDBNull<string> (),
                DateAsked = (DateTime)row[7],
                DateFirstAnswered = row[8].FromDBNull<DateTime?> (),
                DateLastAnswered = row[9].FromDBNull<DateTime?> (),
                Asker = row[10].FromDBNull<string> (),
                AcceptedAnswerId = row[11].FromDBNull<int?> (),
                AcceptedAnswer = row[12].FromDBNull<string> (),
                TopAnswerId = row[13].FromDBNull<int?> (),
                TopAnswer = row[14].FromDBNull<string> (),
                Favorites = (int)row[15],
            };
            return result;
        }
    }
}
