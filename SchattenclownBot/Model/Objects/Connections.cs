using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Persistence.Connection;
using System;
using System.Reflection;

namespace SchattenclownBot.Model.Objects
{
   public class Connections
   {
      public string DiscordBotKey { get; set; }
      public string DiscordBotDebug { get; set; }
      public string MySqlConStr { get; set; }
      public string MySqlConStrDebug { get; set; }
      public string MySqlApiConStr { get; set; }
      public string AcoustIdApiKey { get; set; }
      public SpotifyOAuth2 Token { get; set; }
      public string YouTubeApiKey { get; set; }

      public class SpotifyOAuth2
      {
         public string ClientId { get; set; }
         public string ClientSecret { get; set; }
      }

      public static Connections GetConnections()
      {
         try
         {
            return CsvConnections.ReadAll();
         }
         catch (Exception ex)
         {
            CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
            return null;
         }
      }
   }
}
