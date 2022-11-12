using MySql.Data.MySqlClient;
using SchattenclownBot.Model.AsyncFunction;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Persistence
{
   internal class DB_API
   {
      internal static List<API> GET()
      {
         string sql = "SELECT * FROM `db_SelfApi`.`CommandRequests`";

         List<API> aPIGETs = new();
         MySqlConnection mySqlConnection = DbConnection.OpenAPIDb();
         MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sql, mySqlConnection);

         if (mySqlDataReader != null)
         {
            while (mySqlDataReader.Read())
            {
               API aPI = new()
               {
                  CommandRequestID = mySqlDataReader.GetInt32("CommandRequestID"),
                  RequestDiscordUserId = mySqlDataReader.GetUInt64("RequestDiscordUserId"),
                  RequestSecretKey = mySqlDataReader.GetUInt64("RequestSecretKey"),
                  RequestTimeStamp = mySqlDataReader.GetDateTime("RequestTimeStamp"),
                  Command = mySqlDataReader.GetString("Command")
               };

               aPIGETs.Add(aPI);
            }
         }

         DbConnection.CloseDb(mySqlConnection);
         return aPIGETs;
      }
      internal static void DELETE(int commandRequestID)
      {
         string sql = $"DELETE FROM `db_SelfApi`.`CommandRequests` WHERE (`CommandRequests`.`CommandRequestID` = '{commandRequestID}') AND (`CommandRequests`.`RequestSecretKey` = 42069)";
         DbConnection.ExecuteNonQueryAPI(sql);
      }
   }
}

