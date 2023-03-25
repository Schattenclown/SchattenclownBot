using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.AppCommands.Music;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class ApiHandler
   {
      public static void RunInnerHandlerAsync()
      {
         Task.Run(Function);
      }

      private static async Task Function()
      {
         List<DiscordGuild> guildList;
         do
         {
            guildList = Bot.DiscordClient.Guilds.Values.ToList();
            await Task.Delay(1000);
         } while (guildList.Count == 0);

         while (true)
         {
            List<API> aPiObjects = API.ReadAll();
            foreach (API aPiItem in aPiObjects)
            {
               switch (aPiItem.Command)
               {
                  case "NextTrack":
                     NextTrackRequestApi(aPiItem);
                     break;
                  case "PreviousTrack":
                     PreviousTrackRequestApi(aPiItem);
                     break;
                  case "RequestUserName":
                     RequestUserNameAnswer(aPiItem);
                     CwLogger.Write($"Request for username from {aPiItem.Data} with Id: {aPiItem.RequestDiscordUserId} at {aPiItem.RequestTimeStamp} with Ip: {aPiItem.RequesterIp}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.DarkYellow);
                     break;
                  case "RequestDiscordGuildname":
                     RequestDiscordGuildname(aPiItem);
                     CwLogger.Write($"Request for guildname from {aPiItem.Data} with Id: {aPiItem.RequestDiscordUserId} at {aPiItem.RequestTimeStamp} with Ip: {aPiItem.RequesterIp}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.DarkYellow);
                     break;
                  case "API_PlayRequest":
                     API_PlayRequest(aPiItem);
                     break;
                  case "ShufflePlayRequest":
                     API_ShufflePlayRequest(aPiItem);
                     break;
                  case "ShuffleRequest":
                     API_ShuffleRequest(aPiItem);
                     break;
               }
            }

            await Task.Delay(100);
         }
      }

      public static async void RequestUserNameAnswer(API aPi)
      {
         API.DELETE(aPi.CommandRequestId);
         DiscordUser discordUser = await Bot.DiscordClient.GetUserAsync(aPi.RequestDiscordUserId);
         aPi.Data = discordUser.Username;
         aPi.Command = "RequestUserNameAnswer";
         API.Response(aPi);
      }

      public static void RequestDiscordGuildname(API aPi)
      {
         API.DELETE(aPi.CommandRequestId);

         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         {
            foreach (DiscordMember dummy in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPi.RequestDiscordUserId))
            {
               aPi.Data = guildItem.Name;
               break;
            }
         }

         aPi.Command = "RequestDiscordGuildnameAnswer";
         API.Response(aPi);
      }

      public static void NextTrackRequestApi(API aPi)
      {
         APIRequests.API_NextTrackRequest(aPi);
      }

      public static void PreviousTrackRequestApi(API aPi)
      {
         APIRequests.API_PreviousTrackRequest(aPi);
      }

      public static void API_PlayRequest(API aPi)
      {
         API.DELETE(aPi.CommandRequestId);
         APIRequests.API_PlayRequest(aPi);
      }

      public static void API_ShufflePlayRequest(API aPi)
      {
         API.DELETE(aPi.CommandRequestId);
         APIRequests.API_ShufflePlayRequest(aPi);
      }

      public static void API_ShuffleRequest(API aPi)
      {
         API.DELETE(aPi.CommandRequestId);
         Task aPiShuffleTask = APIRequests.API_ShuffleRequest(aPi);
         if (aPiShuffleTask.IsCompleted)
         {
            //maybe POST Success
         }
      }
   }
}