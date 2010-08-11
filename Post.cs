using System;
using System.Collections;

namespace PivotStack
{
    public struct Post
    {
        public int Id;
        public string Name;
        public string Description;
        public int Score;
        public int Views;
        public int Answers;
        public string Tags;
        public DateTime DateAsked;
        public DateTime? DateFirstAnswered;
        public DateTime? DateLastAnswered;
        public string Asker;
        public int? AcceptedAnswerId;
        public string AcceptedAnswer;
        public int? TopAnswerId;
        public string TopAnswer;
        public int Favorites;

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
                Asker = (string)row[10],
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
            else
            {
                return (T)data;
            }
        }
    }
}
