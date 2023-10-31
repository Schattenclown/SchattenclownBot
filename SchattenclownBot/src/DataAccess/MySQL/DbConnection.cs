using System;
using MySql.Data.MySqlClient;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot.DataAccess.MySQL
{
    internal class DbConnection
    {
        private static string _token = "";

        public static MySqlConnection OpenDb()
        {
            _token = DiscordBot.Connections.MySqlConStr;
#if DEBUG
            _token = DiscordBot.Connections.MySqlConStrDebug;
#endif

            MySqlConnection connection = new(_token);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                ConsoleLogger.WriteLine(ex);
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
                ConsoleLogger.WriteLine(ex);
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
                ConsoleLogger.WriteLine(ex);
                Reset.RestartProgram();
                throw;
            }
        }
    }
}