using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Model.AsyncFunction;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence.DB;

namespace SchattenclownBot.Model.Objects
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
         DB_BotAlarmClocks.Add(botAlarmClock);
         BotAlarmClocksDbRefresh();
      }

      public static void Delete(BotAlarmClock botAlarmClock)
      {
         DB_BotAlarmClocks.Delete(botAlarmClock);
         BotAlarmClocksDbRefresh();
      }

      public static void BotAlarmClockRunAsync()
      {
         DB_BotAlarmClocks.CreateTable_BotAlarmClock();
         BotAlarmClockList = DB_BotAlarmClocks.ReadAll();

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
                  BotAlarmClockList = DB_BotAlarmClocks.ReadAll();
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
         BotAlarmClockList = DB_BotAlarmClocks.ReadAll();
      }
   }
}