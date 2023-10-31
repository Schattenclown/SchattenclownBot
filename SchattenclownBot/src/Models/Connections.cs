using System;
using SchattenclownBot.DataAccess.CSV;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Models
{
    public class Connections
    {
        public string DiscordBotKey { get; set; }
        public string DiscordBotDebug { get; set; }
        public string MssqlConnectionString { get; set; }
        public string MySqlConStr { get; set; }
        public string MySqlConStrDebug { get; set; }
        public string MySqlApiConStr { get; set; }
        public string MySqlApiConStrDebug { get; set; }
        public string AcoustIdApiKey { get; set; }
        public SpotifyOAuth2 SpotifyToken { get; set; }
        public TwitchOAuth2 TwitchToken { get; set; }
        public string YouTubeApiKey { get; set; }

        public static Connections GetConnections()
        {
            try
            {
                return CsvConnections.ReadAll();
            }
            catch (Exception ex)
            {
                ConsoleLogger.WriteLine(ex);
                return null;
            }
        }

        public class SpotifyOAuth2
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        public class TwitchOAuth2
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
    }
}