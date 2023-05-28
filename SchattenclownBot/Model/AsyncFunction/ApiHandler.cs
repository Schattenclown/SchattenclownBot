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
            try
            {
               List<Api> aPiObjects = Api.ReadAll();
               foreach (Api aPiItem in aPiObjects)
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

                  DateTime dateTimeCompare = DateTime.Now.AddMinutes(-1);
                  if (aPiItem.RequestTimeStamp < dateTimeCompare)
                  {
                     Api.DELETE(aPiItem.CommandRequestId);
                  }
               }

               await Task.Delay(100);
            }
            catch (Exception e)
            {
               CwLogger.Write($"{e.Message}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.Red);
            }
         }
      }

      public static async void RequestUserNameAnswer(Api aPi)
      {
         Api.DELETE(aPi.CommandRequestId);
         DiscordUser discordUser = await Bot.DiscordClient.GetUserAsync(aPi.RequestDiscordUserId);
         aPi.Data = discordUser.Username;
         aPi.Command = "RequestUserNameAnswer";
         Api.Response(aPi);
      }

      public static void RequestDiscordGuildname(Api aPi)
      {
         Api.DELETE(aPi.CommandRequestId);

         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         {
            foreach (DiscordMember dummy in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPi.RequestDiscordUserId))
            {
               aPi.Data = guildItem.Name;
               break;
            }
         }

         aPi.Command = "RequestDiscordGuildnameAnswer";
         Api.Response(aPi);
      }

      public static void NextTrackRequestApi(Api aPi)
      {
         ApiRequests.API_NextTrackRequest(aPi);
      }

      public static void PreviousTrackRequestApi(Api aPi)
      {
         ApiRequests.API_PreviousTrackRequest(aPi);
      }

      public static void API_PlayRequest(Api aPi)
      {
         Api.DELETE(aPi.CommandRequestId);
         ApiRequests.API_PlayRequest(aPi);
      }

      public static void API_ShufflePlayRequest(Api aPi)
      {
         Api.DELETE(aPi.CommandRequestId);
         ApiRequests.API_ShufflePlayRequest(aPi);
      }

      public static void API_ShuffleRequest(Api aPi)
      {
         Api.DELETE(aPi.CommandRequestId);
         Task aPiShuffleTask = ApiRequests.API_ShuffleRequest(aPi);
         if (aPiShuffleTask.IsCompleted)
         {
            //maybe POST Success
         }
      }
   }
}