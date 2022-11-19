using System;
using System.Reflection;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.Persistence.Connection;

internal class DB_API_Connection
{
   private static string _apiToken = "";

   public static MySqlConnection API_OpenDB()
   {
      _apiToken = Bot.Connections.MySqlAPIConStr;
#if DEBUG
      _apiToken = Bot.Connections.MySqlAPIConStrDebug;
#endif
      MySqlConnection connection = new(_apiToken);

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

   public static void API_CloseDB(MySqlConnection connection)
   {
      connection.Close();
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

      API_CloseDB(connection);
   }

   public static MySqlDataReader API_ExecuteReader(string sql, MySqlConnection connection)
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
}