using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence.DB_API
{
   internal class DbApiRequests
   {
      internal static List<Api> ReadAll()
      {
         const string sql = "SELECT * FROM CommandRequests";

         List<Api> aPiObjects = new();
         MySqlConnection mySqlConnection = DbApiConnection.API_OpenDB();
         MySqlDataReader mySqlDataReader = DbApiConnection.API_ExecuteReader(sql, mySqlConnection);

         if (mySqlDataReader != null)
         {
            while (mySqlDataReader.Read())
            {
               Api aPi = new()
               {
                  CommandRequestId = mySqlDataReader.GetInt32("CommandRequestID"),
                  RequestDiscordUserId = mySqlDataReader.GetUInt64("RequestDiscordUserId"),
                  RequestSecretKey = mySqlDataReader.GetString("RequestSecretKey"),
                  RequestTimeStamp = mySqlDataReader.GetDateTime("RequestTimeStamp"),
                  RequesterIp = mySqlDataReader.GetString("RequesterIP"),
                  Command = mySqlDataReader.GetString("Command"),
                  Data = mySqlDataReader.GetString("Data")
               };

               aPiObjects.Add(aPi);
            }
         }

         DbApiConnection.API_CloseDB(mySqlConnection);
         return aPiObjects;
      }

      internal static void Delete(int commandRequestId)
      {
         string sql = $"DELETE FROM CommandRequests WHERE `CommandRequests`.`CommandRequestID` = '{commandRequestId}'";
         DbApiConnection.API_ExecuteNonQuery(sql);
      }

      public static void Response(Api aPi)
      {
         string sql = "INSERT INTO CommandRequests (`RequestDiscordUserId`, `RequestSecretKey`, `requesterIP`, `Command`, `Data`) " + $"VALUES ({aPi.RequestDiscordUserId}, '{aPi.RequestSecretKey}', '{aPi.RequesterIp}', '{aPi.Command}', '{aPi.Data}')";

         DbApiConnection.API_ExecuteNonQuery(sql);
      }
   }
}