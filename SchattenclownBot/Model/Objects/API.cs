using System;
using System.Collections.Generic;
using SchattenclownBot.Model.Persistence.DB_API;

namespace SchattenclownBot.Model.Objects
{
   internal class Api
   {
      public int CommandRequestId { get; set; }
      public ulong RequestDiscordGuildId { get; set; }
      public ulong RequestDiscordUserId { get; set; }
      public string RequestSecretKey { get; set; }
      public string Username { get; set; }
      public DateTime RequestTimeStamp { get; set; }
      public string RequesterIp { get; set; }
      public string Command { get; set; }
      public string Data { get; set; }

      internal static List<Api> ReadAll()
      {
         return DbApiRequests.ReadAll();
      }

      internal static void DELETE(int commandRequestId)
      {
         DbApiRequests.Delete(commandRequestId);
      }

      public static void Response(Api aPi)
      {
         DbApiRequests.Response(aPi);
      }
   }
}