using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Objects
{
   internal class API
   {
      public int CommandRequestID { get; set; }
      public ulong RequestDiscordUserId { get; set; }
      public ulong RequestSecretKey { get; set; }
      public DateTime RequestTimeStamp { get; set; }
      public string Command { get; set; }
      internal static List<API> GET()
      {
         return DB_API.GET();
      }
      internal static void DELETE(int commandRequestID)
      {
         DB_API.DELETE(commandRequestID);
      }
   }
}
