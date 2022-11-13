using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Discord.AppCommands.Music;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode;
using static SchattenclownBot.Model.Discord.AppCommands.Music.PlayMusic;

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
                        NextTrackRequestApi(item);
                        break;
                     case "PreviousTrack":
                        PreviousTrackRequestApi(item);
                        break;
                     case "RequestUserName":
                        RequestUserNameAnswer(item);
                        CwLogger.Write($"Login from {item.Data} with Id: {item.RequestDiscordUserId} at {item.RequestTimeStamp} with Ip: {item.RequesterIp}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.DarkYellow);
                        break;
                     case "ApiPlay":
                        ApiPlay(item);
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
      public static async Task ApiPlay(Api aPi)
      {
         Api.Delete(aPi.CommandRequestId);
         await PlayMusic.ApiPlay(aPi);
      }
   }
}
