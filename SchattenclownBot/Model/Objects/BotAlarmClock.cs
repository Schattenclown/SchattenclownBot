using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Objects
{
    public class BotAlarmClock
    {
        public int DBEntryID { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }

        public static List<BotAlarmClock> BotAlarmClockList;
        public static void Add(BotAlarmClock botAlarmClock)
        {
            DB_BotAlarmClocks.Add(botAlarmClock);
            BotAlarmClocksDBRefresh();
        }
        public static void Delete(BotAlarmClock botAlarmClock)
        {
            DB_BotAlarmClocks.Delete(botAlarmClock);
            BotAlarmClocksDBRefresh();
        }
        public static async Task BotAlarmClockRunAsync()
        {
            DB_BotAlarmClocks.CreateTable_BotAlarmClock();
            BotAlarmClockList = DB_BotAlarmClocks.ReadAll();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (BotAlarmClock BotAlarmClockItem in BotAlarmClockList)
                    {
                        if (BotAlarmClockItem.NotificationTime < DateTime.Now)
                        {
                            DiscordChannel chn = await Bot.Client.GetChannelAsync(BotAlarmClockItem.ChannelId);
                            DiscordEmbedBuilder eb = new();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{BotAlarmClockItem.MemberId}> Alarm for {BotAlarmClockItem.NotificationTime} rings!");

                            BotAlarmClock.Delete(BotAlarmClockItem);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 30)
                        BotAlarmClockList = DB_BotAlarmClocks.ReadAll();

                    await Task.Delay(1000 * 1);
                }
            });
        }
        public static void BotAlarmClocksDBRefresh()
        {
            BotAlarmClockList = DB_BotAlarmClocks.ReadAll();
        }
    }
}
