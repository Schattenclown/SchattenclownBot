using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Model.AsyncFunction;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence.DB;

namespace SchattenclownBot.Model.Objects
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
         DB_BotTimer.Add(botTimer);
         BotTimerDbRefresh();
      }

      public static void Delete(BotTimer botTimer)
      {
         DB_BotTimer.Delete(botTimer);
         BotTimerDbRefresh();
      }

      public static List<BotTimer> ReadAll()
      {
         return DB_BotTimer.ReadAll();
      }

      public static void BotTimerRunAsync()
      {
         DB_BotTimer.CreateTable_BotTimer();
         BotTimerList = DB_BotTimer.ReadAll();

         Task.Run(async () =>
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
                  BotTimerList = DB_BotTimer.ReadAll();
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
         BotTimerList = DB_BotTimer.ReadAll();
      }
   }
}