using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

using PivotStack.Properties;

namespace PivotStack
{
    public class Program
    {
        internal static readonly string SelectTags = LoadCommandText ("select-tags.sql");
        internal static readonly string SelectPostsByTag = LoadCommandText ("select-posts-by-tag.sql");
        internal static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
        {
#if DEBUG
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
#endif
        };

        public static int Main (string[] args)
        {
            using (var conn = new SqlConnection(Settings.Default.DatabaseConnectionString))
            {
                conn.Open ();
                var tags = EnumerateTags (conn);
                //foreach (var tag in tags)
                var tag = "tips-and-tricks";
                {
                    using (var outputStream 
                        = new FileStream (tag + ".cxml", FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        var parameters = new Dictionary<string, object> { {"@tag", tag} };
                        var posts = EnumerateRecords (conn, SelectPostsByTag, parameters);
                        PivotizeTag (tag, posts, outputStream);
                    }
                }
            }
            return 0;
        }

        internal static void PivotizeTag (string tag, IEnumerable<object[]> posts, Stream destination)
        {
            var doc = new XDocument (
                new XDeclaration ("1.0", "utf-8", "yes"),
                new XElement ("Root")
                /* TODO: initialize template */);
            foreach (var row in posts)
            {
                var element = PivotizePost (row);
                doc.Root.Add (element);
            }
            using (var writer = XmlWriter.Create (destination, WriterSettings))
            {
                doc.Save (writer);
            }
        }

        internal static XNode PivotizePost (object[] row)
        {
            var result = new XElement ("post");

            return result;
        }

        internal static IEnumerable<object[]> EnumerateRecords 
            (SqlConnection conn, string commandText, IDictionary<string, object> parameters)
        {
            using (var command = conn.CreateCommand ())
            {
                command.CommandText = commandText;
                foreach (var pair in parameters)
                {
                    var param = new SqlParameter(pair.Key, pair.Value);
                    command.Parameters.Add (param);
                }

                using (var reader = command.ExecuteReader (CommandBehavior.SingleResult))
                {
                    while (reader.Read ())
                    {
                        var destination = new object[reader.FieldCount];
                        reader.GetValues (destination);
                        yield return destination;
                    }
                }
            }
        }

        internal static IEnumerable<string> EnumerateTags (SqlConnection conn)
        {
            using (var command = conn.CreateCommand ())
            {
                command.CommandText = SelectTags;
                using (var reader = command.ExecuteReader (CommandBehavior.SingleResult))
                {
                    while (reader.Read ())
                    {
                        yield return reader.GetString(0);
                    }
                }
            }
        }

        internal static string LoadCommandText (string commandName)
        {
            var me = Assembly.GetExecutingAssembly ();
            using (var stream = me.GetManifestResourceStream (typeof (Program), commandName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd ();
                return result;
            }
        }

    }
}
