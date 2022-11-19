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

namespace SchattenclownBot.Model.AsyncFunction;

internal class API_Handler
{
   public static void RunInnerHandlerAsync()
   {
      Task.Run(async () =>
      {
         List<DiscordGuild> guildList;
         do
         {
            guildList = Bot.DiscordClient.Guilds.Values.ToList();
            await Task.Delay(1000);
         } while (guildList.Count == 0);

         while (true)
         {
            List<API> aPI_Objects = API.ReadAll();
            foreach (API aPI_Item in aPI_Objects)
            {
               switch (aPI_Item.Command)
               {
                  case "NextTrack":
                     NextTrackRequestApi(aPI_Item);
                     break;
                  case "PreviousTrack":
                     PreviousTrackRequestApi(aPI_Item);
                     break;
                  case "RequestUserName":
                     RequestUserNameAnswer(aPI_Item);
                     CwLogger.Write($"Request for username from {aPI_Item.Data} with Id: {aPI_Item.RequestDiscordUserId} at {aPI_Item.RequestTimeStamp} with Ip: {aPI_Item.RequesterIp}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.DarkYellow);
                     break;
                  case "RequestDiscordGuildname":
                     RequestDiscordGuildname(aPI_Item);
                     CwLogger.Write($"Request for guildname from {aPI_Item.Data} with Id: {aPI_Item.RequestDiscordUserId} at {aPI_Item.RequestTimeStamp} with Ip: {aPI_Item.RequesterIp}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.DarkYellow);
                     break;
                  case "API_PlayRequest":
                     API_PlayRequest(aPI_Item);
                     break;
                  case "ShufflePlayRequest":
                     API_ShufflePlayRequest(aPI_Item);
                     break;
                  case "ShuffleRequest":
                     API_ShuffleRequest(aPI_Item);
                     break;
               }
            }

            await Task.Delay(100);
         }
      });
   }

   public static async void RequestUserNameAnswer(API aPI)
   {
      API.DELETE(aPI.CommandRequestId);
      DiscordUser discordUser = await Bot.DiscordClient.GetUserAsync(aPI.RequestDiscordUserId);
      aPI.Data = discordUser.Username;
      aPI.Command = "RequestUserNameAnswer";
      API.Response(aPI);
   }

   public static void RequestDiscordGuildname(API aPI)
   {
      API.DELETE(aPI.CommandRequestId);

      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
      {
         foreach (DiscordMember member in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPI.RequestDiscordUserId))
         {
            aPI.Data = guildItem.Name;
            break;
         }
      }

      aPI.Command = "RequestDiscordGuildnameAnswer";
      API.Response(aPI);
   }

   public static void NextTrackRequestApi(API aPI)
   {
      PlayMusic.NextTrackRequestApi(aPI);
   }

   public static void PreviousTrackRequestApi(API aPI)
   {
      PlayMusic.PreviousTrackRequestApi(aPI);
   }

   public static void API_PlayRequest(API aPI)
   {
      API.DELETE(aPI.CommandRequestId);
      PlayMusic.API_PlayRequest(aPI);
   }

   public static void API_ShufflePlayRequest(API aPI)
   {
      API.DELETE(aPI.CommandRequestId);
      PlayMusic.API_ShufflePlayRequest(aPI);
   }

   public static void API_ShuffleRequest(API aPI)
   {
      API.DELETE(aPI.CommandRequestId);
      Task aPI_ShuffleTask = PlayMusic.API_Shuffle(aPI);
      if (aPI_ShuffleTask.IsCompleted)
      {
         //maybe POST Success
      }
   }
}