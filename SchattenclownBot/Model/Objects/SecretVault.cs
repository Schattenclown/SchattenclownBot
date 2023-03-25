using SchattenclownBot.Model.Persistence.DB_API;

namespace SchattenclownBot.Model.Objects
{
   internal class SecretVault
   {
      public int SecretsId { get; set; }
      public ulong DiscordGuildId { get; set; }
      public ulong DiscordUserId { get; set; }
      public string Username { get; set; }
      public string SecretKey { get; set; }

      public static void Register(SecretVault secretVault)
      {
         DbApiSecretVault.Register(secretVault);
      }

      public static SecretVault Read(ulong discordGuildId)
      {
         return DbApiSecretVault.Read(discordGuildId);
      }
   }
}