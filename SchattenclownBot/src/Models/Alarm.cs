using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Persistence.DataAccess.MSSQL;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Models
{
    public class Alarm
    {
        [NotMapped]
        public static List<Alarm> AlarmList;

        [Key]
        public int ID { get; set; }

        [Required]
        public DateTime NotificationTime { get; set; }

        [Required]
        public ulong ChannelId { get; set; }

        [Required]
        public ulong MemberId { get; set; }

        public void Add(Alarm alarm)
        {
            new AlarmDBA().Add(alarm);
            BotAlarmClocksDbRefresh();
        }

        public void Delete(Alarm alarm)
        {
            new AlarmDBA().Delete(alarm);
            BotAlarmClocksDbRefresh();
        }

        public List<Alarm> ReadAll()
        {
            return new AlarmDBA().ReadAll();
        }

        public void RunAsync()
        {
            new CustomLogger().Information("Starting Alarm...", ConsoleColor.Green);
            AlarmList = new Alarm().ReadAll();

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (Alarm botAlarmClockItem in AlarmList)
                    {
                        if (botAlarmClockItem.NotificationTime < DateTime.Now)
                        {
                            DiscordChannel chn = await DiscordBot.DiscordClient.GetChannelAsync(botAlarmClockItem.ChannelId);
                            DiscordEmbedBuilder eb = new();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{botAlarmClockItem.MemberId}> AlarmAC for {botAlarmClockItem.NotificationTime} rings!");

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
                        AlarmList = new Alarm().ReadAll();
                    }

                    await Task.Delay(1000 * 1);
                    if (!LastMinuteCheck.BotAlarmClockRunAsync)
                    {
                        LastMinuteCheck.BotAlarmClockRunAsync = true;
                    }
                }
            });
        }

        public void BotAlarmClocksDbRefresh()
        {
            AlarmList = new Alarm().ReadAll();
        }
    }
}