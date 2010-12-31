using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

using SoftwareNinjas.Core;

namespace PivotStack.Repositories
{
    public abstract class DatabaseRepositoryBase
    {
        private readonly IDbConnection _connection;
        public static readonly IDictionary<string, object> EmptyParameterDictionary =
            new ReadOnlyDictionary<string, object> (new Dictionary<string, object> ());

        protected DatabaseRepositoryBase(IDbConnection connection)
        {
            _connection = connection;
        }

        public IDbConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        internal IEnumerable<object[]> EnumerateRecords
            (string commandText)
        {
            return EnumerateRecords (commandText, EmptyParameterDictionary);
        }

        internal IEnumerable<object[]> EnumerateRecords
            (string commandText, IDictionary<string, object> parameters)
        {
            using (var command = _connection.CreateCommand ())
            {
                command.CommandText = commandText;
                command.CommandTimeout = 0;
                foreach (var pair in parameters)
                {
                    var param = command.CreateParameter ();
                    param.ParameterName = pair.Key;
                    param.Value = pair.Value;
                    command.Parameters.Add (param);
                }

                using (var reader = command.ExecuteReader (CommandBehavior.SingleResult))
                {
                    Debug.Assert(reader != null);
                    while (reader.Read ())
                    {
                        var destination = new object[reader.FieldCount];
                        reader.GetValues (destination);
                        yield return destination;
                    }
                }
            }
        }

        internal static string LoadCommandText (string commandName)
        {
            using (var stream = AssemblyExtensions.OpenScopedResourceStream<DatabaseRepositoryBase> (commandName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd ();
                return result;
            }
        }
    }
}
