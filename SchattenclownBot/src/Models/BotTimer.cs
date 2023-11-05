using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.DataAccess.MySQL.Services;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Models
{
    public class BotTimer
    {
        public static List<BotTimer> BotTimerList;
        public int DbEntryId { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }

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

        public static void RunAsync()
        {
            CustomLogger.Information("Starting BotTimer...", ConsoleColor.Green);
            DbBotTimer.CreateTable();
            BotTimerList = DbBotTimer.ReadAll();

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (BotTimer botTimerItem in BotTimerList)
                    {
                        if (botTimerItem.NotificationTime < DateTime.Now)
                        {
                            DiscordChannel chn = await DiscordBot.DiscordClient.GetChannelAsync(botTimerItem.ChannelId);
                            DiscordEmbedBuilder eb = new()
                            {
                                        Color = DiscordColor.Red
                            };
                            eb.WithDescription($"<@{botTimerItem.MemberId}> Timer for {botTimerItem.NotificationTime} is up!");

                            Delete(botTimerItem);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 15)
                    {
                        BotTimerList = DbBotTimer.ReadAll();
                    }

                    await Task.Delay(1000 * 1);
                    if (!LastMinuteCheck.BotTimerRunAsync)
                    {
                        LastMinuteCheck.BotTimerRunAsync = true;
                    }
                }
            });
        }

        public static void BotTimerDbRefresh()
        {
            BotTimerList = DbBotTimer.ReadAll();
        }
    }
}