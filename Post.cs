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

        public static Post Load (IList row)
        {
            var result = new Post
            {
                Id = (int)row[0],
                Name = (string)row[1],
                Description = (string)row[2],
                Score = (int)row[3],
                Views = (int)row[4],
                Answers = (int)row[5],
                Tags = Value<string>(row[6]),
                DateAsked = (DateTime)row[7],
                DateFirstAnswered = Value<DateTime?>(row[8]),
                DateLastAnswered = Value<DateTime?>(row[9]),
                Asker = Value<string>(row[10]),
                AcceptedAnswerId = Value<int?>(row[11]),
                AcceptedAnswer = Value<string>(row[12]),
                TopAnswerId = Value<int?>(row[13]),
                TopAnswer = Value<string>(row[14]),
                Favorites = (int)row[15],
            };
            return result;
        }

        internal static T Value<T>(object data)
        {
            if (data == DBNull.Value)
            {
                return default (T);
            }
            return (T)data;
        }
    }
}
