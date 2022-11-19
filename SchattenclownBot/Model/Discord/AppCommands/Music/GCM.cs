using DisCatSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   public class GCM
   {
      public DiscordGuild DiscordGuild { get; set; }
      public DiscordChannel DiscordChannel { get; set; }
      public DiscordMember DiscordMember { get; set; }
      public GCM(DiscordGuild discordGuild, DiscordChannel discordChannel, DiscordMember discordMember)
      {
         DiscordGuild = discordGuild;
         DiscordChannel = discordChannel;
         DiscordMember = discordMember;
      }

      public GCM()
      {

      }
   }
}
