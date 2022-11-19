using System;
using System.Collections.Generic;
using SchattenclownBot.Model.Persistence.DB_API;

namespace SchattenclownBot.Model.Objects;

internal class API
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

   internal static List<API> ReadAll()
   {
      return DB_API_Requests.ReadAll();
   }

   internal static void DELETE(int commandRequestId)
   {
      DB_API_Requests.DELETE(commandRequestId);
   }

   public static void Response(API aPi)
   {
      DB_API_Requests.Response(aPi);
   }
}