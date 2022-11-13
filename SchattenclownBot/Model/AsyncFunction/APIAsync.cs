using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.AppCommands.Music;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class ApiAsync
   {
      public static async Task ReadFromApiAsync()
      {
         await Task.Run(async () =>
         {
            List<DiscordGuild> guildList;
            do
            {
               guildList = Bot.DiscordClient.Guilds.Values.ToList();
               await Task.Delay(1000);
            } while (guildList.Count == 0);

            while (true)
            {
               List<Api> aPiObjects = Api.Get();
               foreach (var item in aPiObjects)
               {
                  switch (item.Command)
                  {
                     case "NextTrack":
                        PlayMusic.NextTrackRequestApi(item);
                        break;
                     case "PreviousTrack":
                        PlayMusic.PreviousTrackRequestApi(item);
                        break;
                     case "RequestUserName":
                        RequestUserNameAnswer(item);
                        CwLogger.Write($"Login from {item.Data} with Id: {item.RequestDiscordUserId} at {item.RequestTimeStamp} with Ip: {item.RequesterIp}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.DarkYellow);
                        break;
                  }

               }
               await Task.Delay(100);
            }
         });
      }
      public static async void RequestUserNameAnswer(Api aPi)
      {
         Api.Delete(aPi.CommandRequestId);
         DiscordUser discordUser = await Bot.DiscordClient.GetUserAsync(aPi.RequestDiscordUserId);
         aPi.Data = discordUser.Username;
         aPi.Command = "RequestUserNameAnswer";
         Api.Put(aPi);
      }
   }
}
