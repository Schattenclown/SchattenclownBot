using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Persistence
{
   internal class DbApi
   {
      internal static List<Api> Get()
      {
         string sql = "SELECT * FROM `db_SelfApi`.`CommandRequests`";

         List<Api> apiHandles = new();
         MySqlConnection mySqlConnection = DbConnection.OpenApiDb();
         MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sql, mySqlConnection);

         if (mySqlDataReader != null)
         {
            while (mySqlDataReader.Read())
            {
               Api aPi = new()
               {
                  CommandRequestId = mySqlDataReader.GetInt32("CommandRequestID"),
                  RequestDiscordUserId = mySqlDataReader.GetUInt64("RequestDiscordUserId"),
                  RequestSecretKey = mySqlDataReader.GetUInt64("RequestSecretKey"),
                  RequestTimeStamp = mySqlDataReader.GetDateTime("RequestTimeStamp"),
                  RequesterIp = mySqlDataReader.GetString("RequesterIP"),
                  Command = mySqlDataReader.GetString("Command"),
                  Data = mySqlDataReader.GetString("Data")
               };

               apiHandles.Add(aPi);
            }
         }

         DbConnection.CloseDb(mySqlConnection);
         return apiHandles;
      }
      internal static void Delete(int commandRequestId)
      {
         string sql = $"DELETE FROM `db_SelfApi`.`CommandRequests` WHERE (`CommandRequests`.`CommandRequestID` = '{commandRequestId}') AND (`CommandRequests`.`RequestSecretKey` = 42069)";
         DbConnection.ExecuteNonQueryApi(sql);
      }
      public static void Put(Api aPi)
      {
         string sql = "INSERT INTO `db_SelfApi`.`CommandRequests` (`RequestDiscordUserId`, `RequestSecretKey`, `requesterIP`, `Command`, `Data`) " +
                      $"VALUES ({aPi.RequestDiscordUserId}, {aPi.RequestSecretKey}, '{aPi.RequesterIp}', '{aPi.Command}', '{aPi.Data}')";

         DbConnection.ExecuteNonQueryApi(sql);
      }
   }
}

