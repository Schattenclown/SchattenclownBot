using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Persistence.DatabaseAccess;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Models
{
    public class Timer
    {
        [NotMapped]
        public static List<Timer> TimerList;

        [Key]
        public int ID { get; set; }

        [Required]
        public DateTime NotificationTime { get; set; }

        [Required]
        public ulong ChannelID { get; set; }

        [Required]
        public ulong MemberID { get; set; }

        public void Add(Timer timer)
        {
            new TimerDBA().Add(timer);
            BotTimerDbRefresh();
        }

        public void Delete(Timer timer)
        {
            new TimerDBA().Delete(timer);
            BotTimerDbRefresh();
        }

        public List<Timer> ReadAll()
        {
            return new TimerDBA().ReadAll();
        }

        public void RunAsync()
        {
            new CustomLogger().Information("Starting TimerAC...", ConsoleColor.Green);
            TimerList = new Timer().ReadAll();

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (Timer botTimerItem in TimerList)
                    {
                        if (botTimerItem.NotificationTime < DateTime.Now)
                        {
                            DiscordChannel chn = await DiscordBot.DiscordClient.GetChannelAsync(botTimerItem.ChannelID);
                            DiscordEmbedBuilder eb = new()
                            {
                                        Color = DiscordColor.Red
                            };
                            eb.WithDescription($"<@{botTimerItem.MemberID}> TimerAC for {botTimerItem.NotificationTime} is up!");

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
                        TimerList = new Timer().ReadAll();
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
            TimerList = new Timer().ReadAll();
        }
    }
}