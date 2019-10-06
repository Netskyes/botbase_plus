using System;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace Aeon.Internal
{
    public class SQLiteClient
    {
        private SQLiteConnection conn;

        public SQLiteClient(string dbName)
        {
            string path = Path.Combine(Utils.AssemblyDirectory, dbName);
            bool isTableRequired = false;

            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
                isTableRequired = true;
            }

            conn = new SQLiteConnection(string.Format("Data Source={0};Version=3", path));
            conn.Open();

            if (isTableRequired)
            {
                string query1 = "CREATE TABLE packets (" + 
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "length INT NOT NULL," +
                    "bytes BLOB NOT NULL," +
                    "type VARCHAR (100) NOT NULL," +
                    "source VARCHAR (100) NOT NULL," +
                    "endpoint VARCHAR (100) NOT NULL," +
                    "alias VARCHAR (100) NOT NULL" +
                ");";

                ExecuteCommand(Command(query1));
            }
        }

        public SQLiteCommand Command(string query, SQLiteParameter[] queryParams = null)
        {
            SQLiteCommand command = conn.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;

            if (queryParams != null)
            {
                command.Parameters.AddRange(queryParams);
            }
            
            return command;
        }

        public int ExecuteQuery(string query, SQLiteParameter[] queryParams = null)
        {
            return ExecuteCommand(Command(query, queryParams));
        }

        public int ExecuteCommand(SQLiteCommand command)
        {
            return command.ExecuteNonQuery();
        }

        public SQLiteDataReader ExecuteReader(SQLiteCommand command)
        {
            return command.ExecuteReader();
        }

        public int FetchNumRows(SQLiteCommand command)
        {
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
