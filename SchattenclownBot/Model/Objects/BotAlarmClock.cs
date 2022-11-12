// Copyright (c) Schattenclown

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects;

public class BotAlarmClock
{
	public int DbEntryId { get; set; }
	public DateTime NotificationTime { get; set; }
	public ulong ChannelId { get; set; }
	public ulong MemberId { get; set; }

	public static List<BotAlarmClock> BotAlarmClockList { get; set; }

	public static void Add(BotAlarmClock botAlarmClock)
	{
		DbBotAlarmClocks.Add(botAlarmClock);
		BotAlarmClocksDbRefresh();
	}
	public static void Delete(BotAlarmClock botAlarmClock)
	{
		DbBotAlarmClocks.Delete(botAlarmClock);
		BotAlarmClocksDbRefresh();
	}
	public static async Task BotAlarmClockRunAsync()
	{
		DbBotAlarmClocks.CreateTable_BotAlarmClock();
		BotAlarmClockList = DbBotAlarmClocks.ReadAll();

		await Task.Run(async () =>
		{
			while (true)
			{
				foreach (var botAlarmClockItem in BotAlarmClockList)
				{
					if (botAlarmClockItem.NotificationTime < DateTime.Now)
					{
						var chn = await Bot.DiscordClient.GetChannelAsync(botAlarmClockItem.ChannelId);
						DiscordEmbedBuilder eb = new()
						{
							Color = DiscordColor.Red
						};
						eb.WithDescription($"<@{botAlarmClockItem.MemberId}> Alarm for {botAlarmClockItem.NotificationTime} rings!");

						Delete(botAlarmClockItem);
						for (var i = 0; i < 3; i++)
						{
							await chn.SendMessageAsync(eb.Build());
							await Task.Delay(50);
						}
					}
				}

				if (DateTime.Now.Second == 30)
					BotAlarmClockList = DbBotAlarmClocks.ReadAll();

				await Task.Delay(1000 * 1);
				if (!AsyncFunction.LastMinuteCheck.BotAlarmClockRunAsync)
					AsyncFunction.LastMinuteCheck.BotAlarmClockRunAsync = true;
			}
			// ReSharper disable once FunctionNeverReturns
		});
	}
	public static void BotAlarmClocksDbRefresh() => BotAlarmClockList = DbBotAlarmClocks.ReadAll();
}
