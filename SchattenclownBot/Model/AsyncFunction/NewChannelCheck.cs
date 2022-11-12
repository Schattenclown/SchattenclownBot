// Copyright (c) Schattenclown

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

namespace SchattenclownBot.Model.AsyncFunction;

internal class NewChannelCheck
{
	internal static async Task CheckTask(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
	{
		if (eventArgs.Guild.Id == 928930967140331590)
		{
			var guild = client.GetGuildAsync(928930967140331590).Result;
			var channels = guild.GetChannelsAsync().Result;
			List<DiscordChannel> rightChannels = new();

			var parrentchannel = client.GetChannelAsync(928937353593118731).Result;
			var mainChannel = client.GetChannelAsync(1022234777539051590).Result;


			foreach (var channel in channels.Where(x => x.ParentId == 928937353593118731))
			{
				rightChannels.Add(channel);
			}

			var compairInt = 1;
			foreach (var channel in rightChannels)
			{
				if (client.GetChannelAsync(channel.Id).Result.Users.Any())
					compairInt++;
			}

			if (compairInt >= rightChannels.Count)
			{
				var newChannel = guild.CreateChannelAsync("Other", ChannelType.Voice, parrentchannel, Optional<string>.None, 384000).Result;
				rightChannels.Add(newChannel);
			}

			compairInt = 0;
			var oneFree = false;
			foreach (var channel in rightChannels)
			{
				if (channel != mainChannel)
				{
					if (!client.GetChannelAsync(channel.Id).Result.Users.Any())
						compairInt++;

					if (compairInt == 3 || oneFree)
					{
						await channel.DeleteAsync();
						oneFree = true;
					}
				}
			}
		}
	}
}
