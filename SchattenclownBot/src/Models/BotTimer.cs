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
        public List<BotTimer> BotTimerList;
        public int DbEntryId { get; set; }
        public DateTime NotificationTime { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MemberId { get; set; }

        public void Add(BotTimer botTimer)
        {
            new DbBotTimer().Add(botTimer);
            BotTimerDbRefresh();
        }

        public void Delete(BotTimer botTimer)
        {
            new DbBotTimer().Delete(botTimer);
            BotTimerDbRefresh();
        }

        public List<BotTimer> ReadAll()
        {
            return new DbBotTimer().ReadAll();
        }

        public void RunAsync()
        {
            new CustomLogger().Information("Starting BotTimer...", ConsoleColor.Green);
            new DbBotTimer().CreateTable();
            BotTimerList = new DbBotTimer().ReadAll();

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
                        BotTimerList = new DbBotTimer().ReadAll();
                    }

                    await Task.Delay(1000 * 1);
                    if (!LastMinuteCheck.BotTimerRunAsync)
                    {
                        LastMinuteCheck.BotTimerRunAsync = true;
                    }
                }
            });
        }

        public void BotTimerDbRefresh()
        {
            BotTimerList = new DbBotTimer().ReadAll();
        }
    }
}