﻿using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using System;
using System.Reflection;

namespace SchattenclownBot.Model.Persistence.Connection
{
   class DbConnection
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
            CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
            Reset.RestartProgram();
            throw;
         }

         return connection;
      }
        public static MySqlConnection OpenAPIDb()
        {
            _token = Bot.Connections.MySqlAPIConStr;
#if DEBUG
            _token = Bot.Connections.MySqlAPIConStr;
#endif

            MySqlConnection connection = new(_token);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
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
            CWLogger.Write($"{sqlCommand.CommandText}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         }
         CloseDb(connection);
      }
        public static void ExecuteNonQueryAPI(string sql)
        {
            MySqlConnection connection = OpenAPIDb();
            MySqlCommand sqlCommand = new(sql, connection);
            int ret = sqlCommand.ExecuteNonQuery();
            if (ret != -1)
            {
                CWLogger.Write($"{sqlCommand.CommandText}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
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
            CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
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
            CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
            Reset.RestartProgram();
            throw;
         }
      }
   }
}
