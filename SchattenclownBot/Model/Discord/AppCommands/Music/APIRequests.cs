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
   internal static async void NextTrackRequestApi(API aPI)
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

   internal static async void PreviousTrackRequestApi(API aPI)
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

   internal static void API_PlayRequest(API api)
   {
      Main.AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, false);
   }

   internal static void API_ShufflePlayRequest(API api)
   {
      Main.AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, true);
   }

   internal static async Task API_Shuffle(API aPI)
   {
      GMC gMC = GMC.FromDiscordUserID(aPI.RequestDiscordUserId);
      if (gMC == null)
      {
         gMC = GMC.MemberFromID(aPI.RequestDiscordUserId);
         await gMC.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
         return;
      }

      Main.ShuffleQueueTracksAsyncTask(gMC);
   }
}