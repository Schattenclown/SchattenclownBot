using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class NewChannelCheck
   {
      internal static async Task CheckTask(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
      {
         if (eventArgs.Guild.Id == 928930967140331590)
         {
            DiscordGuild guild = client.GetGuildAsync(928930967140331590).Result;
            IReadOnlyList<DiscordChannel> channels = guild.GetChannelsAsync().Result;
            List<DiscordChannel> rightChannels = new();

            DiscordChannel parentChannel = client.GetChannelAsync(928937353593118731).Result;
            DiscordChannel mainChannel = client.GetChannelAsync(1022234777539051590).Result;


            foreach (DiscordChannel channel in channels.Where(x => x.ParentId == 928937353593118731))
            {
               rightChannels.Add(channel);
            }

            int compareInt = 1;
            foreach (DiscordChannel channel in rightChannels)
            {
               if (client.GetChannelAsync(channel.Id).Result.Users.Any())
               {
                  compareInt++;
               }
            }

            if (compareInt >= rightChannels.Count)
            {
               DiscordChannel newChannel = guild.CreateChannelAsync("Other", ChannelType.Voice, parentChannel, Optional<string>.None, 384000).Result;
               rightChannels.Add(newChannel);
            }

            compareInt = 0;
            bool oneFree = false;
            foreach (DiscordChannel channel in rightChannels)
            {
               if (channel != mainChannel)
               {
                  if (!client.GetChannelAsync(channel.Id).Result.Users.Any())
                  {
                     compareInt++;
                  }

                  if (compareInt == 3 || oneFree)
                  {
                     await channel.DeleteAsync();
                     oneFree = true;
                  }
               }
            }
         }
      }
   }
}