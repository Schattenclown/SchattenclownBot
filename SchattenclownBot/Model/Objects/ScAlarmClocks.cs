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
    public class ScAlarmClock
    {
        private static DiscordBot dbot = new DiscordBot();
        public int DBEntryID { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }
        public static void Add(ScAlarmClock alarmClock)
        {
            DB_ScAlarmClocks.Add(alarmClock);
            dbot.ScAlarmClocksDBRefresh();
        }
        public static void Delete(ScAlarmClock alarmClock)
        {
            DB_ScAlarmClocks.Delete(alarmClock);
            dbot.ScAlarmClocksDBRefresh();
        }
        public static List<ScTimer> ReadAll()
        {
            return DB_ScTimers.ReadAll();
        }
    }
}
