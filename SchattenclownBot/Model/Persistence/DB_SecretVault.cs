using SchattenclownBot.Model.Persistence.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchattenclownBot.Model.Objects;
using MySql.Data.MySqlClient;

namespace SchattenclownBot.Model.Persistence
{
   internal class DB_SecretVault
   {
      internal static SecretVault Read(ulong discordUserId)
      {
         string sql = $"SELECT * FROM `db_SelfApi`.`SecretsVault` WHERE DiscordUserId = {discordUserId}";

         SecretVault secretVault = new();
         MySqlConnection mySqlConnection = DB_Connection.API_OpenDB();
         MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sql, mySqlConnection);

         if (mySqlDataReader != null)
         {
            while (mySqlDataReader.Read())
            {
               secretVault = new()
               {
                  DiscordUserId = mySqlDataReader.GetUInt64("DiscordUserId")
               };
            }
         }

         DB_Connection.CloseDB(mySqlConnection);
         return secretVault;
      }

      public static void Register(SecretVault secretVault)
      {
         string sql = $"INSERT INTO `db_SelfApi`.`SecretsVault` (`DiscordGuildId`, `DiscordUserId`, `Username`, `SecretKey`) " +
                      $"VALUES ({secretVault.DiscordGuildId}, {secretVault.DiscordUserId}, '{secretVault.Username}', '{secretVault.SecretKey}')";

         DB_Connection.API_ExecuteNonQuery(sql);
      }
   }
}
