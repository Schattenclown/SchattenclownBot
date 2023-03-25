using System;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.AppCommands.Music.Objects;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   internal class ApiRequests
   {
      internal static void API_PlayRequest(Api api)
      {
         _ = Main.AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, false);
      }

      internal static void API_ShufflePlayRequest(Api api)
      {
         _ = Main.AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, true);
      }

      internal static async void API_NextTrackRequest(Api aPi)
      {
         CwLogger.Write(aPi.RequestTimeStamp + " " + aPi.RequesterIp + " " + aPi.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__5", "").Replace("<", ""), ConsoleColor.DarkYellow);
         Api.DELETE(aPi.CommandRequestId);

         Gmc gMc = Gmc.FromDiscordUserId(aPi.RequestDiscordUserId);
         if (gMc == null)
         {
            gMc = Gmc.MemberFromId(aPi.RequestDiscordUserId);
            await gMc.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }

         Main.PlayNextTrackFromQueue(gMc);
      }

      internal static async void API_PreviousTrackRequest(Api aPi)
      {
         CwLogger.Write(aPi.RequestTimeStamp + " " + aPi.RequesterIp + " " + aPi.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__6", "").Replace("<", ""), ConsoleColor.DarkYellow);
         Api.DELETE(aPi.CommandRequestId);

         Gmc gMc = Gmc.FromDiscordUserId(aPi.RequestDiscordUserId);
         if (gMc == null)
         {
            gMc = Gmc.MemberFromId(aPi.RequestDiscordUserId);
            await gMc.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }

         Main.PlayPreviousTrackFromQueue(gMc);
      }

      internal static async Task API_ShuffleRequest(Api aPi)
      {
         Gmc gMc = Gmc.FromDiscordUserId(aPi.RequestDiscordUserId);
         if (gMc == null)
         {
            gMc = Gmc.MemberFromId(aPi.RequestDiscordUserId);
            await gMc.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }

         _ = Main.ShuffleQueueTracksAsyncTask(gMc, null);
      }
   }
}