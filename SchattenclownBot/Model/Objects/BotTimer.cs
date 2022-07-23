using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Objects
{
    public class BotTimer
    {
        public int DBEntryID { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }
        public static List<BotTimer> botTimerList;
        public static void Add(BotTimer botTimer)
        {
            DB_BotTimer.Add(botTimer);
            BotTimerDBRefresh();
        }
        public static void Delete(BotTimer botTimer)
        {
            DB_BotTimer.Delete(botTimer);
            BotTimerDBRefresh();
        }
        public static List<BotTimer> ReadAll()
        {
            return DB_BotTimer.ReadAll();
        }
        public static async Task BotTimerRunAsync()
        {
            DB_BotTimer.CreateTable_BotTimer();
            botTimerList = DB_BotTimer.ReadAll();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (BotTimer botTimerItem in botTimerList)
                    {
                        if (botTimerItem.NotificationTime < DateTime.Now)
                        {
                            DiscordChannel chn = await Bot.DiscordClient.GetChannelAsync(botTimerItem.ChannelId);
                            DiscordEmbedBuilder eb = new();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{botTimerItem.MemberId}> Timer for {botTimerItem.NotificationTime} is up!");

                            BotTimer.Delete(botTimerItem);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 15)
                        botTimerList = DB_BotTimer.ReadAll();

                    await Task.Delay(1000 * 1);
                }
            });
        }
        public static void BotTimerDBRefresh()
        {
            botTimerList = DB_BotTimer.ReadAll();
        }
    }
}
