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
      public int DbEntryId { get; set; }
      public DateTime NotificationTime { get; set; }
      public ulong ChannelId { get; set; }
      public ulong MemberId { get; set; }

      public static List<BotAlarmClock> BotAlarmClockList;
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
      public static void BotAlarmClockRunAsync()
      {
         DbBotAlarmClocks.CreateTable_BotAlarmClock();
         BotAlarmClockList = DbBotAlarmClocks.ReadAll();

         Task.Run(async () =>
         {
            while (true)
            {
               foreach (BotAlarmClock botAlarmClockItem in BotAlarmClockList)
               {
                  if (botAlarmClockItem.NotificationTime < DateTime.Now)
                  {
                     DiscordChannel chn = await Bot.DiscordClient.GetChannelAsync(botAlarmClockItem.ChannelId);
                     DiscordEmbedBuilder eb = new();
                     eb.Color = DiscordColor.Red;
                     eb.WithDescription($"<@{botAlarmClockItem.MemberId}> Alarm for {botAlarmClockItem.NotificationTime} rings!");

                     BotAlarmClock.Delete(botAlarmClockItem);
                     for (int i = 0; i < 3; i++)
                     {
                        await chn.SendMessageAsync(eb.Build());
                        await Task.Delay(50);
                     }
                  }
               }

               if (DateTime.Now.Second == 30)
                  BotAlarmClockList = DbBotAlarmClocks.ReadAll();

               await Task.Delay(1000 * 1);
               if (!AsyncFunction.LastMinuteCheck.BotAlarmClockRunAsync)
                  AsyncFunction.LastMinuteCheck.BotAlarmClockRunAsync = true;
            }
         });
      }
      public static void BotAlarmClocksDbRefresh()
      {
         BotAlarmClockList = DbBotAlarmClocks.ReadAll();
      }
   }
}
