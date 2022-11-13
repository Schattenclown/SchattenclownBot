using DisCatSharp.Entities;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   internal class QueueCreating
   {
      internal DiscordGuild DiscordGuild { get; set; }
      internal int QueueAmount { get; set; }
      internal int QueueAddedAmount { get; set; }

      internal QueueCreating(DiscordGuild discordGuild, int queueAmount, int queueAddedAmount)
      {
         DiscordGuild = discordGuild;
         QueueAmount = queueAmount;
         QueueAddedAmount = queueAddedAmount;
      }
   }
}
