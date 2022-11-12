using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Objects
{
   internal class Api
   {
      public int CommandRequestId { get; set; }
      public ulong RequestDiscordUserId { get; set; }
      public ulong RequestSecretKey { get; set; }
      public DateTime RequestTimeStamp { get; set; }
      public string RequesterIp { get; set; }
      public string Command { get; set; }
      public string Data { get; set; }
      internal static List<Api> Get()
      {
         return DbApi.Get();
      }
      internal static void Delete(int commandRequestId)
      {
         DbApi.Delete(commandRequestId);
      }
      public static void Put(Api aPi)
      {
         DbApi.Put(aPi);
      }
   }
}
