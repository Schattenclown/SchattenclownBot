using System;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Objects
{
    public class Connections
    {
        public string DiscordBotKey { get; set; }
        public string DiscordBotDebug { get; set; }
        public string MySqlConStr { get; set; }
        public string MySqlConStrDebug { get; set; }

        public static Connections GetConnections()
        {
            try
            {
                return CSV_Connections.ReadAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return null;
            }
        }
    }
}
