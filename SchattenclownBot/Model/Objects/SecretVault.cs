using SchattenclownBot.Model.Persistence.DB_API;

namespace SchattenclownBot.Model.Objects;

internal class SecretVault
{
   public int SecretsID { get; set; }
   public ulong DiscordGuildId { get; set; }
   public ulong DiscordUserId { get; set; }
   public string Username { get; set; }
   public string SecretKey { get; set; }

   public static void Register(SecretVault secretVault)
   {
      DB_API_SecretVault.Register(secretVault);
   }

   public static SecretVault Read(ulong discordGuildId)
   {
      return DB_API_SecretVault.Read(discordGuildId);
   }
}