using System;
using MySql.Data.MySqlClient;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot.DataAccess.MySQL
{
    public class DbConnection
    {
        private static string Token { get; set; } = null!;

        public static MySqlConnection OpenDb()
        {
            Token = DiscordBot.Config["ConnectionStrings:MySql"];
#if DEBUG
            Token = DiscordBot.Config["ConnectionStrings:MySqlDebug"];
#endif

            MySqlConnection connection = new(Token);

            try
            {
                connection.Open();
            }
            catch (Exception exception)
            {
                CustomLogger.Error(exception);
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
            sqlCommand.ExecuteNonQuery();

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
            catch (Exception exception)
            {
                CustomLogger.Error(exception);
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
                CustomLogger.Error(ex);
                Reset.RestartProgram();
                throw;
            }
        }
    }
}