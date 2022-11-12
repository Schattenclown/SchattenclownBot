// Copyright (c) Schattenclown

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.AsyncFunction;

internal class GetItRightMee6
{
	internal static Task ItRight(DiscordClient client, ChannelCreateEventArgs e)
	{
		if (e.Channel.Name.Contains("🥇AFK-Farm#"))
		{
			e.Channel.ModifyAsync(x => x.Bitrate = 256000);
			CWLogger.Write("Bitrate to 256k on" + e.Channel.Name, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		}

		return Task.CompletedTask;
	}

	public static async Task CheckHighQualityAvailable(int executeSecond)
	{
		await Task.Run(async () =>
		{
			while (DateTime.Now.Second != executeSecond)
			{
				await Task.Delay(1000);
			}

			List<DiscordGuild> guildList;
			do
			{
				guildList = Bot.DiscordClient.Guilds.Values.ToList();
				await Task.Delay(1000);
			} while (guildList.Count == 0);

			while (true)
			{
				var bool384KbNotAvailable = false;
				DiscordGuild mainGuild = null;

				foreach (var guildItem in guildList.Where(x => x.Id == 928930967140331590))
				{
					mainGuild = guildItem;
				}

				if (mainGuild == null)
					return;

				IEnumerable<DiscordChannel> discordChannels = mainGuild.Channels.Values.Where(x => x.Type == ChannelType.Voice).ToList();

				foreach (var discordChannelItem in discordChannels)
				{
					try
					{
						if (discordChannelItem.Id != 982330147141218344)
							if (discordChannelItem.Bitrate != 384000)
							{
								await discordChannelItem.ModifyAsync(x => x.Bitrate = 384000);
								CWLogger.Write($"Bit-rate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 384000!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
							}
					}
					catch
					{
						bool384KbNotAvailable = true;
						CWLogger.Write($"Bit-rate 384000 not available for guild", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
						break;
					}
				}

				if (bool384KbNotAvailable)
				{
					foreach (var discordChannelItem in discordChannels)
					{
						try
						{
							if (discordChannelItem.Id != 982330147141218344)
								if (discordChannelItem.Bitrate != 256000)
								{
									await discordChannelItem.ModifyAsync(x => x.Bitrate = 256000);
									CWLogger.Write($"Bit-rate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 256000!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
								}
						}
						catch
						{
							CWLogger.Write($"Bit-rate 256000 not available for guild", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
							break;
						}
					}
				}

				await Task.Delay(1000);
				if (!LastMinuteCheck.CheckHighQualityAvailable)
					LastMinuteCheck.CheckHighQualityAvailable = true;
			}
		});
	}
}
