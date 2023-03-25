using System.Threading;
using DisCatSharp.Entities;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects
{
   internal class DcCancellationTokenItem
   {
      internal DcCancellationTokenItem(DiscordGuild discordGuild, CancellationTokenSource cancellationTokenSource)
      {
         DiscordGuild = discordGuild;
         CancellationTokenSource = cancellationTokenSource;
      }

      internal DiscordGuild DiscordGuild { get; set; }
      internal CancellationTokenSource CancellationTokenSource { get; set; }
   }
}