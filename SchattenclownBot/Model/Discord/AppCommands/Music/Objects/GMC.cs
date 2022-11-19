using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using System.Linq;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects;

public class GMC
{
   public DiscordGuild DiscordGuild { get; set; }
   public DiscordMember DiscordMember { get; set; }
   public DiscordChannel DiscordChannel { get; set; }

   public GMC(DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel discordChannel)
   {
      DiscordGuild = discordGuild;
      DiscordMember = discordMember;
      DiscordChannel = discordChannel;
   }

   public static GMC FromDiscordUserID(ulong discordUserID)
   {
      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
      {
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == discordUserID))
         {
            GMC gMC = new()
            {
               DiscordGuild = guildItem,
               DiscordMember = memberItem,
               DiscordChannel = memberItem.VoiceState.Channel
            };
            return gMC;
         }
      }

      return null;
   }

   public static GMC MemberFromID(ulong discordUserID)
   {
      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
      {
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == discordUserID))
         {
            GMC gMC = new()
            {
               DiscordGuild = guildItem,
               DiscordMember = memberItem
            };
            return gMC;
         }
      }

      return null;
   }

   public GMC()
   {
   }
}