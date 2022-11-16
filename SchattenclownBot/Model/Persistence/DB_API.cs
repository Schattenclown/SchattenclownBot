using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Persistence
{
   internal class DB_API
   {
      internal static List<API> ReadAll()
      {
         const string sql = "SELECT * FROM `db_SelfApi`.`CommandRequests`";

         List<API> aPI_Objects = new();
         MySqlConnection mySqlConnection = DB_Connection.API_OpenDB();
         MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sql, mySqlConnection);

         if (mySqlDataReader != null)
         {
            while (mySqlDataReader.Read())
            {
               API aPI = new()
               {
                  CommandRequestId = mySqlDataReader.GetInt32("CommandRequestID"),
                  RequestDiscordUserId = mySqlDataReader.GetUInt64("RequestDiscordUserId"),
                  RequestSecretKey = mySqlDataReader.GetString("RequestSecretKey"),
                  RequestTimeStamp = mySqlDataReader.GetDateTime("RequestTimeStamp"),
                  RequesterIp = mySqlDataReader.GetString("RequesterIP"),
                  Command = mySqlDataReader.GetString("Command"),
                  Data = mySqlDataReader.GetString("Data")
               };

               aPI_Objects.Add(aPI);
            }
         }

         DB_Connection.CloseDB(mySqlConnection);
         return aPI_Objects;
      }
      internal static void DELETE(int commandRequestId)
      { 
         string sql = $"DELETE FROM `db_SelfApi`.`CommandRequests` WHERE `CommandRequests`.`CommandRequestID` = '{commandRequestId}'";
         DB_Connection.API_ExecuteNonQuery(sql);
      }
      public static void Response(API aPI)
      {
         string sql = "INSERT INTO `db_SelfApi`.`CommandRequests` (`RequestDiscordUserId`, `RequestSecretKey`, `requesterIP`, `Command`, `Data`) " +
                      $"VALUES ({aPI.RequestDiscordUserId}, '{aPI.RequestSecretKey}', '{aPI.RequesterIp}', '{aPI.Command}', '{aPI.Data}')";

         DB_Connection.API_ExecuteNonQuery(sql);
      }
   }
}

