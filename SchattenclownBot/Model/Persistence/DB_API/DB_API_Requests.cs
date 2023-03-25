using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence.DB_API
{
   internal class DB_API_Requests
   {
      internal static List<API> ReadAll()
      {
         const string sql = "SELECT * FROM CommandRequests";

         List<API> aPI_Objects = new();
         MySqlConnection mySqlConnection = DB_API_Connection.API_OpenDB();
         MySqlDataReader mySqlDataReader = DB_API_Connection.API_ExecuteReader(sql, mySqlConnection);

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

         DB_API_Connection.API_CloseDB(mySqlConnection);
         return aPI_Objects;
      }

      internal static void DELETE(int commandRequestId)
      {
         string sql = $"DELETE FROM CommandRequests WHERE `CommandRequests`.`CommandRequestID` = '{commandRequestId}'";
         DB_API_Connection.API_ExecuteNonQuery(sql);
      }

      public static void Response(API aPI)
      {
         string sql = "INSERT INTO CommandRequests (`RequestDiscordUserId`, `RequestSecretKey`, `requesterIP`, `Command`, `Data`) " + $"VALUES ({aPI.RequestDiscordUserId}, '{aPI.RequestSecretKey}', '{aPI.RequesterIp}', '{aPI.Command}', '{aPI.Data}')";

         DB_API_Connection.API_ExecuteNonQuery(sql);
      }
   }
}