using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using System;
using System.Reflection;

namespace SchattenclownBot.Model.Persistence.Connection
{
   class DB_Connection
   {
      private static string _token = "";
      public static MySqlConnection OpenDB()
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
      public static MySqlConnection API_OpenDB()
      {
         _token = Bot.Connections.MySqlApiConStr;
#if DEBUG
         _token = Bot.Connections.MySqlApiConStr;
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
      public static void CloseDB(MySqlConnection connection)
      {
         connection.Close();
      }
      public static void ExecuteNonQuery(string sql)
      {
         MySqlConnection connection = OpenDB();
         MySqlCommand sqlCommand = new(sql, connection);
         int ret = sqlCommand.ExecuteNonQuery();
         if (ret != -1)
         {
            CwLogger.Write($"{sqlCommand.CommandText}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         }
         CloseDB(connection);
      }
      public static void API_ExecuteNonQuery(string sql)
      {
         MySqlConnection connection = API_OpenDB();
         MySqlCommand sqlCommand = new(sql, connection);
         int ret = sqlCommand.ExecuteNonQuery();
         if (ret != -1)
         {
            CwLogger.Write($"{sqlCommand.CommandText}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         }
         CloseDB(connection);
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
