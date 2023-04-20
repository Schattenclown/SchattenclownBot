using System.Threading;
using DisCatSharp.Entities;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects
{
   internal class DcCancellationTokenItem
   {
      internal DcCancellationTokenItem(DiscordGuild discordGuild, CancellationTokenSource cancellationTokenSource, bool isRepeat)
      {
         DiscordGuild = discordGuild;
         CancellationTokenSource = cancellationTokenSource;
         IsRepeat = isRepeat;
      }

      internal DiscordGuild DiscordGuild { get; set; }
      internal CancellationTokenSource CancellationTokenSource { get; set; }
      public bool IsRepeat { get; set; }
   }
}