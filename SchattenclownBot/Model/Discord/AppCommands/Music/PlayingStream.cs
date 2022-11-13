using DisCatSharp.Entities;
using System.IO;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   internal class PlayingStream
   {
      public DiscordGuild DiscordGuild { get; set; }
      public Stream Stream { get; set; }
      internal PlayingStream(DiscordGuild discordGuild, Stream stream)
      {
         DiscordGuild = discordGuild;
         Stream = stream;
      }
   }
}
