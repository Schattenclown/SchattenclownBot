using MySql.Data.MySqlClient;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using System;

namespace SchattenclownBot.Model.Persistence.Connection
{
    class DbConnection
    {
        private static string _token = "";
        private static int _virgin;
        public static void SetDb()
        {
            Connections connections = Connections.GetConnections();
            _token = connections.MySqlConStr;
#if DEBUG
            _token = connections.MySqlConStrDebug;
#endif
        }
        public static MySqlConnection OpenDb()
        {
            if (_virgin == 0)
                SetDb();
            _virgin = 69;

            MySqlConnection connection = new(_token);

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

            return connection;
        }
        public static void CloseDb(MySqlConnection connection)
        {
            connection.Close();
        }
        public static void ExecuteNonQuery(string sql)
        {
            MySqlConnection connection = OpenDb();
            MySqlCommand sqlCommand = new(sql, connection);
            int ret = sqlCommand.ExecuteNonQuery();
            if (ret != -1)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{sqlCommand.CommandText}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            CloseDb(connection);
        }
        public static MySqlDataReader ExecuteReader(string sql, MySqlConnection connection)
        {
            MySqlCommand sqlCommand = new(sql, connection);
            try
            {
                MySqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                return sqlDataReader;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ConsoleStringFormatter.Center("DB IS DEAD");
                Reset.RestartProgram();
                throw;
            }
        }
        public static int ExecuteScalarCount(string sql, MySqlConnection connection)
        {
            MySqlCommand sqlCommand = new(sql, connection);
            try
            {
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
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
