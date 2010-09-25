using System;
using NUnit.Framework;

namespace PivotStack.Test
{
    [TestFixture]
    public class PostTest
    {
        private const int PostId = 3232;
        private const string PostName = "What are the best Excel tips?";
        private const string PostDescription = "<p>What are your best tips/not so known features of excel?</p>";
        private const int PostScore = 7;
        private const int PostViews = 761;
        private const int PostAnswers = 27;
        private const string PostTags = "<excel><tips-and-tricks>";
        private static readonly DateTime PostDateAsked = new DateTime (2009, 07, 15, 18, 36, 28);
        private static readonly DateTime PostDateFirstAnswered = new DateTime (2009, 07, 15, 18, 41, 08);
        private static readonly DateTime PostDateLastAnswered = new DateTime (2010, 06, 16, 09, 46, 07);
        private const string PostAsker = "Bob";
        private const int PostAcceptedAnswerId = 3274;
        private const string PostAcceptedAnswer = "<p>My best advice for Excel...</p>";
        private const int PostTopAnswerId = 21231;
        private const string PostTopAnswer = @"<p><a href=""http://techrageo.us/2007/09/23/in-cell-spreadsheet-graphs/"" rel=""nofollow"">In-cell graphs</a>, using REPT...";
        private const int PostFavorites = 10;

        internal static readonly Post AnsweredAndAccepted = new Post
        {
            Id = PostId,
            Name = PostName,
            Description = PostDescription,
            Score = PostScore,
            Views = PostViews,
            Answers = PostAnswers,
            Tags = PostTags,
            DateAsked = PostDateAsked,
            DateFirstAnswered = PostDateFirstAnswered,
            DateLastAnswered = PostDateLastAnswered,
            Asker = PostAsker,
            AcceptedAnswerId = PostAcceptedAnswerId,
            AcceptedAnswer = PostAcceptedAnswer,
            TopAnswerId = PostTopAnswerId,
            TopAnswer = PostTopAnswer,
            Favorites = PostFavorites,
        };

        [Test]
        public void Load ()
        {
            // arrange
            var row = new object[] 
            {
                PostId,
                PostName,
                PostDescription,
                PostScore,
                PostViews,
                PostAnswers,
                PostTags,
                PostDateAsked,
                PostDateFirstAnswered,
                PostDateLastAnswered,
                PostAsker,
                PostAcceptedAnswerId,
                PostAcceptedAnswer,
                PostTopAnswerId,
                PostTopAnswer,
                PostFavorites,
            };

            // act
            var actual = Post.Load (row);

            // assert
            Assert.AreEqual (PostId, actual.Id);
            Assert.AreEqual (PostName, actual.Name);
            Assert.AreEqual (PostDescription, actual.Description);
            Assert.AreEqual (PostScore, actual.Score);
            Assert.AreEqual (PostViews, actual.Views);
            Assert.AreEqual (PostAnswers, actual.Answers);
            Assert.AreEqual (PostTags, actual.Tags);
            Assert.AreEqual (PostDateAsked, actual.DateAsked);
            Assert.AreEqual (PostDateFirstAnswered, actual.DateFirstAnswered);
            Assert.AreEqual (PostDateLastAnswered, actual.DateLastAnswered);
            Assert.AreEqual (PostAsker, actual.Asker);
            Assert.AreEqual (PostAcceptedAnswerId, actual.AcceptedAnswerId);
            Assert.AreEqual (PostAcceptedAnswer, actual.AcceptedAnswer);
            Assert.AreEqual (PostTopAnswerId, actual.TopAnswerId);
            Assert.AreEqual (PostTopAnswer, actual.TopAnswer);
            Assert.AreEqual (PostFavorites, actual.Favorites);
        }

        [Test]
        public void Load_NullAsker ()
        {
            // arrange
            var row = new object[] 
            {
                PostId,
                PostName,
                PostDescription,
                PostScore,
                PostViews,
                PostAnswers,
                PostTags,
                PostDateAsked,
                PostDateFirstAnswered,
                PostDateLastAnswered,
                DBNull.Value,
                PostAcceptedAnswerId,
                PostAcceptedAnswer,
                PostTopAnswerId,
                PostTopAnswer,
                PostFavorites,
            };

            // act
            var actual = Post.Load (row);

            // assert
            Assert.AreEqual (PostId, actual.Id);
            Assert.AreEqual (PostName, actual.Name);
            Assert.AreEqual (PostDescription, actual.Description);
            Assert.AreEqual (PostScore, actual.Score);
            Assert.AreEqual (PostViews, actual.Views);
            Assert.AreEqual (PostAnswers, actual.Answers);
            Assert.AreEqual (PostTags, actual.Tags);
            Assert.AreEqual (PostDateAsked, actual.DateAsked);
            Assert.AreEqual (PostDateFirstAnswered, actual.DateFirstAnswered);
            Assert.AreEqual (PostDateLastAnswered, actual.DateLastAnswered);
            Assert.AreEqual (null, actual.Asker);
            Assert.AreEqual (PostAcceptedAnswerId, actual.AcceptedAnswerId);
            Assert.AreEqual (PostAcceptedAnswer, actual.AcceptedAnswer);
            Assert.AreEqual (PostTopAnswerId, actual.TopAnswerId);
            Assert.AreEqual (PostTopAnswer, actual.TopAnswer);
            Assert.AreEqual (PostFavorites, actual.Favorites);
        }
    }
}
