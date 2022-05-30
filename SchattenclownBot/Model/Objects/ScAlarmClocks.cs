using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects
{
    public class ScAlarmClock
    {
        public int DBEntryID { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }

        public static List<ScAlarmClock> scAlarmClocks;
        public static void Add(ScAlarmClock alarmClock)
        {
            DB_ScAlarmClocks.Add(alarmClock);
            ScAlarmClocksDBRefresh();
        }
        public static void Delete(ScAlarmClock alarmClock)
        {
            DB_ScAlarmClocks.Delete(alarmClock);
            ScAlarmClocksDBRefresh();
        }
        public static List<ScTimer> ReadAll()
        {
            return DB_ScTimers.ReadAll();
        }
        public static async Task ScAlarmClocksRunAsync()
        {
            DB_ScAlarmClocks.CreateTable_ScAlarmClocks();
            scAlarmClocks = DB_ScAlarmClocks.ReadAll();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var scAlarmClock in scAlarmClocks)
                    {
                        if (scAlarmClock.NotificationTime < DateTime.Now)
                        {
                            var chn = await Bot.Client.GetChannelAsync(scAlarmClock.ChannelId);
                            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{scAlarmClock.MemberId}> Alarm for {scAlarmClock.NotificationTime} rings!");

                            ScAlarmClock.Delete(scAlarmClock);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 30)
                        scAlarmClocks = DB_ScAlarmClocks.ReadAll();

                    await Task.Delay(1000 * 1);
                }
            });
        }
        public static void ScAlarmClocksDBRefresh()
        {
            scAlarmClocks = DB_ScAlarmClocks.ReadAll();
        }
    }
}
