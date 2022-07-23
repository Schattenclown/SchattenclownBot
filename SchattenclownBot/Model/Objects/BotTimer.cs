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
        public int DbEntryId { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }
        public static List<BotTimer> BotTimerList;
        public static void Add(BotTimer botTimer)
        {
            DbBotTimer.Add(botTimer);
            BotTimerDbRefresh();
        }
        public static void Delete(BotTimer botTimer)
        {
            DbBotTimer.Delete(botTimer);
            BotTimerDbRefresh();
        }
        public static List<BotTimer> ReadAll()
        {
            return DbBotTimer.ReadAll();
        }
        public static async Task BotTimerRunAsync()
        {
            DbBotTimer.CreateTable_BotTimer();
            BotTimerList = DbBotTimer.ReadAll();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (BotTimer botTimerItem in BotTimerList)
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
                        BotTimerList = DbBotTimer.ReadAll();

                    await Task.Delay(1000 * 1);
                }
                // ReSharper disable once FunctionNeverReturns
            });
        }
        public static void BotTimerDbRefresh()
        {
            BotTimerList = DbBotTimer.ReadAll();
        }
    }
}
