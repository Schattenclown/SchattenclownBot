using SchattenclownBot.Model.Persistence.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects
{
   internal class SecretVault
   {
      public int SecretsID { get; set; }
      public ulong DiscordGuildId { get; set; }
      public ulong DiscordUserId { get; set; }
      public string Username { get; set; }
      public string SecretKey { get; set; }

      public static void Register(SecretVault secretVault)
      {
         DB_SecretVault.Register(secretVault);
      }

      public static SecretVault Read(ulong discordGuildId)
      {
         return DB_SecretVault.Read(discordGuildId);
      }
   }
}
