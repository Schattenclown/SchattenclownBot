using System.Linq;
using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects
{
   public class Gmc
   {
      public Gmc(DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel discordChannel)
      {
         DiscordGuild = discordGuild;
         DiscordMember = discordMember;
         DiscordChannel = discordChannel;
      }

      public Gmc()
      {
      }

      public DiscordGuild DiscordGuild { get; set; }
      public DiscordMember DiscordMember { get; set; }
      public DiscordChannel DiscordChannel { get; set; }

      public static Gmc FromDiscordUserId(ulong discordUserId)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         {
            foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == discordUserId))
            {
               Gmc gMc = new()
               {
                  DiscordGuild = guildItem, DiscordMember = memberItem, DiscordChannel = memberItem.VoiceState.Channel
               };
               return gMc;
            }
         }

         return null;
      }

      public static Gmc MemberFromId(ulong discordUserId)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         {
            foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == discordUserId))
            {
               Gmc gMc = new()
               {
                  DiscordGuild = guildItem, DiscordMember = memberItem
               };
               return gMc;
            }
         }

         return null;
      }
   }
}