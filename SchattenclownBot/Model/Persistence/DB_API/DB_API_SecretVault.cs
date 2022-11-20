using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence.DB_API;

internal class DB_API_SecretVault
{
   internal static SecretVault Read(ulong discordUserId)
   {
      string sql = $"SELECT * FROM SecretsVault WHERE DiscordUserId = {discordUserId}";

      SecretVault secretVault = new();
      MySqlConnection mySqlConnection = DB_API_Connection.API_OpenDB();
      MySqlDataReader mySqlDataReader = DB_API_Connection.API_ExecuteReader(sql, mySqlConnection);

      if (mySqlDataReader != null)
         while (mySqlDataReader.Read())
         {
            secretVault = new SecretVault
            {
               DiscordUserId = mySqlDataReader.GetUInt64("DiscordUserId")
            };
         }

      DB_API_Connection.API_CloseDB(mySqlConnection);
      return secretVault;
   }

   public static void Register(SecretVault secretVault)
   {
      string sql = "INSERT INTO SecretsVault (`DiscordGuildId`, `DiscordUserId`, `Username`, `SecretKey`) " + $"VALUES ({secretVault.DiscordGuildId}, {secretVault.DiscordUserId}, '{secretVault.Username}', '{secretVault.SecretKey}')";

      DB_API_Connection.API_ExecuteNonQuery(sql);
   }
}