using System;
using MySql.Data.MySqlClient;
using SchattenclownBot.Utils;

namespace SchattenclownBot.DataAccess.MySQL
{
    public class DbConnection
    {
        public string Token { get; set; } = null!;

        public MySqlConnection OpenDb()
        {
            Token = Program.Config["ConnectionStrings:MySql"];
#if DEBUG
            Token = Program.Config["ConnectionStrings:MySqlDebug"];
#endif

            MySqlConnection connection = new(Token);

            try
            {
                connection.Open();
            }
            catch (Exception exception)
            {
                new CustomLogger().Error(exception);
                new Reset().RestartProgram();
                throw;
            }

            return connection;
        }

        public void CloseDb(MySqlConnection connection)
        {
            connection.Close();
        }

        public void ExecuteNonQuery(string sql)
        {
            MySqlConnection connection = OpenDb();
            MySqlCommand sqlCommand = new(sql, connection);
            sqlCommand.ExecuteNonQuery();

            CloseDb(connection);
        }

        public MySqlDataReader ExecuteReader(string sql, MySqlConnection connection)
        {
            MySqlCommand sqlCommand = new(sql, connection);
            try
            {
                MySqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                return sqlDataReader;
            }
            catch (Exception exception)
            {
                new CustomLogger().Error(exception);
                new Reset().RestartProgram();
                throw;
            }
        }

        public int ExecuteScalarCount(string sql, MySqlConnection connection)
        {
            MySqlCommand sqlCommand = new(sql, connection);
            try
            {
                int count = Convert.ToInt32(sqlCommand.ExecuteScalar());
                return count;
            }
            catch (Exception ex)
            {
                new CustomLogger().Error(ex);
                new Reset().RestartProgram();
                throw;
            }
        }
    }
}