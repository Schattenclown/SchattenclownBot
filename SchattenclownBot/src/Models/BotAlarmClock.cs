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
    public class BotAlarmClock
    {
        public static List<BotAlarmClock> BotAlarmClockList;
        public int DbEntryId { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }

        public static void Add(BotAlarmClock botAlarmClock)
        {
            DbBotAlarmClocks.Add(botAlarmClock);
            BotAlarmClocksDbRefresh();
        }

        public static void Delete(BotAlarmClock botAlarmClock)
        {
            DbBotAlarmClocks.Delete(botAlarmClock);
            BotAlarmClocksDbRefresh();
        }

        public static void RunAsync()
        {
            CustomLogger.Information("Starting BotAlarmClock...", ConsoleColor.Green);
            DbBotAlarmClocks.CreateTable();
            BotAlarmClockList = DbBotAlarmClocks.ReadAll();

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (BotAlarmClock botAlarmClockItem in BotAlarmClockList)
                    {
                        if (botAlarmClockItem.NotificationTime < DateTime.Now)
                        {
                            DiscordChannel chn = await DiscordBot.DiscordClient.GetChannelAsync(botAlarmClockItem.ChannelId);
                            DiscordEmbedBuilder eb = new();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{botAlarmClockItem.MemberId}> Alarm for {botAlarmClockItem.NotificationTime} rings!");

                            Delete(botAlarmClockItem);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 30)
                    {
                        BotAlarmClockList = DbBotAlarmClocks.ReadAll();
                    }

                    await Task.Delay(1000 * 1);
                    if (!LastMinuteCheck.BotAlarmClockRunAsync)
                    {
                        LastMinuteCheck.BotAlarmClockRunAsync = true;
                    }
                }
            });
        }

        public static void BotAlarmClocksDbRefresh()
        {
            BotAlarmClockList = DbBotAlarmClocks.ReadAll();
        }
    }
}