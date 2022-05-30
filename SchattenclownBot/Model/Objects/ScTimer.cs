using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects
{
    public class ScTimer
    {
        public int DBEntryID { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }
        public static List<ScTimer> scTimers;
        public static void Add(ScTimer timer)
        {
            DB_ScTimers.Add(timer);
            ScTimersDBRefresh();
        }
        public static void Delete(ScTimer timer)
        {
            DB_ScTimers.Delete(timer);
            ScTimersDBRefresh();
        }
        public static List<ScTimer> ReadAll()
        {
            return DB_ScTimers.ReadAll();
        }
        public static async Task ScTimersRunAsync()
        {
            DB_ScTimers.CreateTable_ScTimers();
            scTimers = DB_ScTimers.ReadAll();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var scTimer in scTimers)
                    {
                        if (scTimer.NotificationTime < DateTime.Now)
                        {
                            var chn = await Bot.Client.GetChannelAsync(scTimer.ChannelId);
                            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{scTimer.MemberId}> Timer for {scTimer.NotificationTime} is up!");

                            ScTimer.Delete(scTimer);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 15)
                        scTimers = DB_ScTimers.ReadAll();

                    await Task.Delay(1000 * 1);
                }
            });
        }
        public static void ScTimersDBRefresh()
        {
            scTimers = DB_ScTimers.ReadAll();
        }
    }
}
