using DisCatSharp.Entities;
using System.Threading;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   internal class DcCancellationTokenItem
   {
      internal DiscordGuild DiscordGuild { get; set; }
      internal CancellationTokenSource CancellationTokenSource { get; set; }

      internal DcCancellationTokenItem(DiscordGuild discordGuild, CancellationTokenSource cancellationTokenSource)
      {
         DiscordGuild = discordGuild;
         CancellationTokenSource = cancellationTokenSource;
      }

      internal DcCancellationTokenItem()
      {

      }
   }
}
