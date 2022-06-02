using System;

using MySql.Data.MySqlClient;

using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Persistence.Connection
{
    class DB_Connection
    {
        private static string token = "";
        private static int virgin = 0;
        public static void SetDB()
        {
            var connections = Connections.GetConnections();
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

            var connection = new MySqlConnection(token);
            do
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    ConsoleStringFormatter.Center("DB IS DEAD");
                    Reset.RestartProgram();
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
            var connection = OpenDB();
            var sqlCommand = new MySqlCommand(sql, connection);
            var ret = sqlCommand.ExecuteNonQuery();
            if (ret != -1)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{sqlCommand.CommandText}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            CloseDB(connection);
        }
        public static MySqlDataReader ExecuteReader(string sql, MySqlConnection connection)
        {
            var sqlCommand = new MySqlCommand(sql, connection);
            try
            {
                var sqlDataReader = sqlCommand.ExecuteReader();
                return sqlDataReader;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                ConsoleStringFormatter.Center("DB IS DEAD");
                Reset.RestartProgram();
                throw;
            }
        }
        public static int ExecuteScalarCount(string sql, MySqlConnection connection)
        {
            var sqlCommand = new MySqlCommand(sql, connection);
            try
            {
                var count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ConsoleStringFormatter.Center("DB IS DEAD");
                Reset.RestartProgram();
                throw;
            }
        }
    }
}
