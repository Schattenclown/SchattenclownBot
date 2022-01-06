using DisCatSharp;
using DisCatSharp.Entities;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SchattenclownBot.Model.Discord;

namespace SchattenclownBot.Model.Objects
{
    public class ScTimer
    {
        private static DiscordBot dbot = new DiscordBot();
        public int DBEntryID { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }
        public static void Add(ScTimer timer)
        {
            DB_ScTimers.Add(timer);
        }
        public static void Delete(ScTimer timer)
        {
            DB_ScTimers.Delete(timer);
        }
        public static List<ScTimer> ReadAll()
        {
            return DB_ScTimers.ReadAll();
        }
    }
}
