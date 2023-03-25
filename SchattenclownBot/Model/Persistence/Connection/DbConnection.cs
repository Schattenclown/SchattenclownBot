using System;
using System.Reflection;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.Persistence.Connection
{
   internal class DbConnection
   {
      private static string _token = "";

      public static MySqlConnection OpenDb()
      {
         _token = Bot.Connections.MySqlConStr;
#if DEBUG
      _token = Bot.Connections.MySqlConStrDebug;
#endif

         MySqlConnection connection = new(_token);

         try
         {
            connection.Open();
         }
         catch (Exception ex)
         {
            CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
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
            CwLogger.Write($"{sqlCommand.CommandText}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.DarkCyan);
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
            CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
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
            CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
            Reset.RestartProgram();
            throw;
         }
      }
   }
}