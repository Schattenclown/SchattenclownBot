using System;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.AppCommands.Music.Objects;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Discord.AppCommands.Music;

internal class APIRequests
{
   internal static void API_PlayRequest(API api)
   {
      _ = Main.AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, false);
   }

   internal static void API_ShufflePlayRequest(API api)
   {
      _ = Main.AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, true);
   }

   internal static async void API_NextTrackRequest(API aPI)
   {
      CwLogger.Write(aPI.RequestTimeStamp + " " + aPI.RequesterIp + " " + aPI.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__5", "").Replace("<", ""), ConsoleColor.DarkYellow);
      API.DELETE(aPI.CommandRequestId);

      GMC gMC = GMC.FromDiscordUserID(aPI.RequestDiscordUserId);
      if (gMC == null)
      {
         gMC = GMC.MemberFromID(aPI.RequestDiscordUserId);
         await gMC.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
         return;
      }

      Main.PlayNextTrackFromQueue(gMC);
   }

   internal static async void API_PreviousTrackRequest(API aPI)
   {
      CwLogger.Write(aPI.RequestTimeStamp + " " + aPI.RequesterIp + " " + aPI.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__6", "").Replace("<", ""), ConsoleColor.DarkYellow);
      API.DELETE(aPI.CommandRequestId);

      GMC gMC = GMC.FromDiscordUserID(aPI.RequestDiscordUserId);
      if (gMC == null)
      {
         gMC = GMC.MemberFromID(aPI.RequestDiscordUserId);
         await gMC.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
         return;
      }

      Main.PlayPreviousTrackFromQueue(gMC);
   }

   internal static async Task API_ShuffleRequest(API aPI)
   {
      GMC gMC = GMC.FromDiscordUserID(aPI.RequestDiscordUserId);
      if (gMC == null)
      {
         gMC = GMC.MemberFromID(aPI.RequestDiscordUserId);
         await gMC.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
         return;
      }

      _ = Main.ShuffleQueueTracksAsyncTask(gMC, null);
   }
}