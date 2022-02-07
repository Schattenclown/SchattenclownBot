using MySql.Data.MySqlClient;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using System;

namespace SchattenclownBot.Model.Persistence.Connection
{
    class DB_Connection
    {
        private static string token = "";
        private static int virgin = 0;
        public static void SetDB()
        {
            Connections connections = Connections.GetConnections();
            token = connections.MySqlConStr;
#if DEBUG
            token = connections.MySqlConStrDebug;
#endif
        }
        public static MySqlConnection OpenDB()
        {
            if (virgin == 0)
                SetDB();
            virgin = 69;

            MySqlConnection connection = new MySqlConnection(token);
            do
            {
                try
                {
                    connection.Open();
                }
                catch
                {
                    Reset.RestartProgram(new Exception("DB DEAD"));
                    throw;
                }
            } while (connection == null);

            return connection;
        }
        public static void CloseDB(MySqlConnection connection)
        {
            connection.Close();
        }
        public static void ExecuteNonQuery(string sql)
        {
            MySqlConnection connection = OpenDB();
            MySqlCommand sqlCommand = new MySqlCommand(sql, connection);
            int ret = sqlCommand.ExecuteNonQuery();
            if (ret != -1)
                Console.WriteLine("DEBUG: DB -1");
            CloseDB(connection);
        }

        public static MySqlDataReader ExecuteReader(string sql, MySqlConnection connection)
        {
            MySqlCommand sqlCommand = new MySqlCommand(sql, connection);
            try
            {
                MySqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                return sqlDataReader;
            }
            catch
            {
                Reset.RestartProgram(new Exception("DB DEAD"));
                throw;
            }
        }
    }
}
